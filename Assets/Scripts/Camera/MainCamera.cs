using UnityEngine;
using System.Collections;
using Kino;
using System;
using Random = UnityEngine.Random;
using System.Linq;

// original code on github here: https://gist.github.com/Jellybit/9f6182c25bceac06db31

public class MainCamera : BaseCamera
{
    public static MainCamera instance;

    [Header("Basic Setup")]
    [Tooltip("Drag your player, or whatever object you want the camera to track here. If you need to get the player by name, there's a line in the code you can uncomment.")]
    public Player player;

    [Tooltip("Should be at least 1.1, as for this to work, the camera has to move faster than the player. Otherwise, it behaves as if the camera is locked to the player.")]
    [Range(1, 10)]
    public float scrollMultiplierX = 1.8f;
    [Range(1, 10)]
    public float scrollMultiplierY = 1f;
    [Space(10)]
    [Tooltip("The player will be kept within this area on the screen. If you have trouble visualizing it, turn on the Debug Visuals below and press play to see it.")]
    public Vector2 movementWindowSize = new Vector2(8, 6);

    [Tooltip("If the root of your character is at the feet, you can set this offset to half the player's height to compensate. You can also just use it to keep the box low to the ground or whatever you like.")]
    public Vector2 windowOffset;

    // Activate your position limitations for the Y axis by turning this on.

    [Header("Camera Movement Boundaries")]
    [Tooltip("Turn this on to have the camera use the positional limits set below. Set both limits on an axis to the same number to lock the camera so that it only moves on the other axis.")]
    public bool limitCameraMovementX = false;
    public bool limitCameraMovementY = false;
    [Space(10)]
    [Tooltip("Set the leftmost position you want the camera to be able to go.")]
    public float limitLeft;
    [Tooltip("Set the rightmost position you want the camera to be able to go.")]
    public float limitRight;
    [Space(10)]
    [Tooltip("Set the lowest position you want the camera to be able to go.")]
    public float limitBottom;
    [Tooltip("Set the highest position you want the camera to be able to go.")]
    public float limitTop;


    [Header("Debug Visuals")]
    [Tooltip("Draws a debug box on the screen while the game is running so you can see the boundaries against the player. Red boundaries mean that they are being ignored due to the following options.")]
    public bool showDebugBoxes = false;

    [HideInInspector]
    public bool activeTracking = true;

    private Vector3 cameraPosition;
    private Vector3 playerPosition;
    private Vector3 previousPlayerPosition;
    private Rect windowRect;

    private Vector3 _shakeOffset;
    private Vector3 _lastShakeOffset;

    private IEnumerator _limitTween;
    private bool _tweening;
    public bool tweening
    {
        get
        {
            return _tweening;
        }
    }

    private bool _zooming;
    private IEnumerator _shakeCoroutine;

    private AnalogGlitch _analogGlitch;
    private DigitalGlitch _digitalGlitch;
    private bool _glitchFading;

    public bool requireUpdate;

    public float yLookMod;
    private float _lastYLookMod;

    public Action OnFrameFinished;

    private AudioListener _audioListener;
    public AudioListener audioListener { get { return _audioListener; } }

    private bool _pixelPerfect
    {
        get
        {
            if (SaveGameManager.instance && SaveGameManager.instance.saveFileData != null)
            {
                return SaveGameManager.instance.saveFileData.pixelPerfect;
            }
            else
            {
                return false;
            }
        }
    }

    public RenderTexture renderT384;
    public RenderTexture renderT768;
    public Material renderMat384;
    public Material renderMat768;
    public MeshRenderer screenMesh;

    private void Awake()
    {
        instance = this;
        camera = GetComponent<Camera>();
        _analogGlitch = GetComponent<AnalogGlitch>();
        _digitalGlitch = GetComponent<DigitalGlitch>();
        _audioListener = GetComponentInChildren<AudioListener>();
    }

    private void Start()
    {
        if (!player) { player = PlayerManager.instance.players.FirstOrDefault(); }

        if(SaveGameManager.instance)
        {
            AudioListener.volume = SaveGameManager.instance.saveFileData.soundVolume;
        }

        cameraPosition = transform.position;

        if (player == null)
            Debug.LogError("You have to let the CameraControl script know which object is your player.");

        previousPlayerPosition = player.transform.position;

        ValidateLeftAndRightLimits();
        ValidateTopAndBottomLimits();

        //These are the root x/y coordinates that we will use to create our boundary rectangle.
        //Starts at the lower left, and takes the offset into account.
        float windowAnchorX = cameraPosition.x - movementWindowSize.x / 2 + windowOffset.x;
        float windowAnchorY = cameraPosition.y - movementWindowSize.y / 2 + windowOffset.y;

        //From our anchor point, we set the size of the window based on the public variable above.
        windowRect = new Rect(windowAnchorX, windowAnchorY, movementWindowSize.x, movementWindowSize.y);

        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.gameMode == GameMode.Spooky)
        {
            SetSpooky();
        }

        if (!_pixelPerfect && screenMesh)
        {
            camera.targetTexture = renderT768;
            screenMesh.material = renderMat768;
        }
    }

    public void SetPixelPerfect()
    {
        if (screenMesh)
        {
            if (_pixelPerfect)
            {
                if (orthographicSize <= defaultSize)
                {
                    camera.targetTexture = renderT384;
                    screenMesh.material = renderMat384;
                }
                else
                {
                    camera.targetTexture = renderT768;
                    screenMesh.material = renderMat768;
                }
            }
            else
            {
                camera.targetTexture = renderT768;
                screenMesh.material = renderMat768;
            }
        }
    }

    private void LateUpdate()
    {        
        if (!player) { player = PlayerManager.instance.players.FirstOrDefault(); }

        playerPosition = player.transform.position;
        cameraPosition = transform.position - _lastShakeOffset;
        cameraPosition.y -= _lastYLookMod;

        var currentOrtho = orthographicSize;

        if (activeTracking && (playerPosition != previousPlayerPosition || !requireUpdate))
        {
            //Get the distance of the player from the camera.
            Vector3 playerPositionDifference = playerPosition - previousPlayerPosition;

            //Move the camera this direction, but faster than the player moved.
            Vector3 multipliedDifference = playerPositionDifference;
            if (!tweening || !DeathmatchManager.instance) //helps make transitons smoother in Deathmatch
            {
                multipliedDifference.x *= scrollMultiplierX;
                multipliedDifference.y *= scrollMultiplierY;
            }

            cameraPosition.x += multipliedDifference.x;
            if (!player.controller2D.bottomEdge.near)
            {
                cameraPosition.y += multipliedDifference.y;
            }

            //updating our movement window root location based on the current camera position
            windowRect.x = cameraPosition.x - movementWindowSize.x / 2 + windowOffset.x;
            windowRect.y = cameraPosition.y - movementWindowSize.y / 2 + windowOffset.y;
            windowRect.width = movementWindowSize.x;
            windowRect.height = movementWindowSize.y;

            // We may have overshot the boundaries, or the player just may have been moving too 
            // fast/popped into another place. This corrects for those cases, and snaps the 
            // boundary to the player.
            if (!windowRect.Contains(playerPosition))
            {
                Vector3 positionDifference = playerPosition - cameraPosition;
                positionDifference.x -= windowOffset.x;
                positionDifference.y -= windowOffset.y;

                //I made a function to figure out how much to move in order to snap the boundary to the player.
                cameraPosition.x += DifferenceOutOfBounds(positionDifference.x, movementWindowSize.x);
                cameraPosition.y += DifferenceOutOfBounds(positionDifference.y, movementWindowSize.y);
            }
        }

        // Here we clamp the desired position into the area declared in the limit variables.
        if (limitCameraMovementY)
        {
            var bottomLimit = limitBottom + currentOrtho;
            var topLimit = limitTop - currentOrtho;
            cameraPosition.y = Mathf.Clamp(cameraPosition.y, bottomLimit, topLimit);
            cameraPosition.y = Mathf.Clamp(cameraPosition.y + yLookMod, bottomLimit, topLimit);
            _lastYLookMod = yLookMod;
        }

        if (limitCameraMovementX)
        {
			float halfWidth = this.halfWidth;
            cameraPosition.x = Mathf.Clamp(cameraPosition.x, limitLeft + halfWidth, limitRight - halfWidth);
        }
        
        // and now we're updating the camera position using what came of all the calculations above.
        transform.position = cameraPosition + _shakeOffset;

        if(_audioListener) { _audioListener.transform.position = playerPosition; }

        previousPlayerPosition = playerPosition;           

        // This draws the camera boundary rectangle
        if (showDebugBoxes) DrawDebugBox();

        requireUpdate = false;

        if (OnFrameFinished != null) { OnFrameFinished(); }
    }

    private static float DifferenceOutOfBounds(float differenceAxis, float windowAxis)
    {
        if (Mathf.Abs(differenceAxis) <= windowAxis / 2)
            return 0f;
        else
            return differenceAxis - (windowAxis / 2) * Mathf.Sign(differenceAxis);
    }

    private void ValidateTopAndBottomLimits()
    {
        if (limitTop < limitBottom)
        {
            Debug.LogError("You have set the limitBottom (" + limitBottom + ") to a higher number than limitTop (" + limitTop + "). This makes no sense as the top has to be higher than the bottom.");
            Debug.LogError("I'm correcting this for you, but please make sure the bottom is under the top next time. If you meant to lock the camera, please set top and bottom limits to the same number.");

            float tempFloat = limitTop;

            limitTop = limitBottom;
            limitBottom = tempFloat;
        }
    }

    private void ValidateLeftAndRightLimits()
    {
        if (limitRight < limitLeft)
        {
            Debug.LogError("You have set the limitLeft (" + limitLeft + ") to a higher number than limitRight (" + limitRight + "). This makes no sense as it puts the left limit to the right of the right limit.");
            Debug.LogError("I'm correcting this for you, but please make sure the left limit is to the left of the right limit. If you meant to lock the camera, please set left and right limits to the same number.");

            float tempFloat = limitRight;

            limitRight = limitLeft;
            limitLeft = tempFloat;
        }
    }

    private void DrawDebugBox()
    {
        Vector3 cameraPos = transform.position;

        //These are the root x/y coordinates that we will use to create our boundary rectangle.
        //Starts at the lower left, and takes the offset into account.
        float windowAnchorX = cameraPosition.x - movementWindowSize.x / 2 + windowOffset.x;
        float windowAnchorY = cameraPosition.y - movementWindowSize.y / 2 + windowOffset.y;

        //From our anchor point, we set the size of the window based on the public variable above.
        windowRect = new Rect(windowAnchorX, windowAnchorY, movementWindowSize.x, movementWindowSize.y);

        //This will draw the boundaries you are tracking in green, and boundaries you are ignoring in red.
        Color xColorA;
        Color xColorB;
        Color yColorA;
        Color yColorB;

        if (!activeTracking || limitCameraMovementY && cameraPos.x <= limitLeft)
            xColorA = Color.red;
        else
            xColorA = Color.green;

        if (!activeTracking || limitCameraMovementY && cameraPos.x >= limitRight)
            xColorB = Color.red;
        else
            xColorB = Color.green;

        if (!activeTracking || limitCameraMovementY && cameraPos.y <= limitBottom)
            yColorA = Color.red;
        else
            yColorA = Color.green;

        if (!activeTracking || limitCameraMovementY && cameraPos.y >= limitTop)
            yColorB = Color.red;
        else
            yColorB = Color.green;

        Vector3 actualWindowCorner1 = new Vector3(windowRect.xMin, windowRect.yMin, 0);
        Vector3 actualWindowCorner2 = new Vector3(windowRect.xMax, windowRect.yMin, 0);
        Vector3 actualWindowCorner3 = new Vector3(windowRect.xMax, windowRect.yMax, 0);
        Vector3 actualWindowCorner4 = new Vector3(windowRect.xMin, windowRect.yMax, 0);

        Debug.DrawLine(actualWindowCorner1, actualWindowCorner2, yColorA);
        Debug.DrawLine(actualWindowCorner2, actualWindowCorner3, xColorB);
        Debug.DrawLine(actualWindowCorner3, actualWindowCorner4, yColorB);
        Debug.DrawLine(actualWindowCorner4, actualWindowCorner1, xColorA);

        // And now we display the camera limits. If the camera is inactive, they will show in red.
        // There is an x in the middle of the screen to show what hits against the limit.
        if (limitCameraMovementY || limitCameraMovementX)
        {
            Color limitColor;

            if (!activeTracking)
                limitColor = Color.red;
            else
                limitColor = Color.cyan;

            Vector3 limitCorner1 = new Vector3(limitLeft, limitBottom, 0);
            Vector3 limitCorner2 = new Vector3(limitRight, limitBottom, 0);
            Vector3 limitCorner3 = new Vector3(limitRight, limitTop, 0);
            Vector3 limitCorner4 = new Vector3(limitLeft, limitTop, 0);

            Debug.DrawLine(limitCorner1, limitCorner2, limitColor);
            Debug.DrawLine(limitCorner2, limitCorner3, limitColor);
            Debug.DrawLine(limitCorner3, limitCorner4, limitColor);
            Debug.DrawLine(limitCorner4, limitCorner1, limitColor);

            //And a little center point

            Vector3 centerMarkCorner1 = new Vector3(cameraPos.x - 0.1f, cameraPos.y + 0.1f, 0);
            Vector3 centerMarkCorner2 = new Vector3(cameraPos.x + 0.1f, cameraPos.y - 0.1f, 0);
            Vector3 centerMarkCorner3 = new Vector3(cameraPos.x - 0.1f, cameraPos.y - 0.1f, 0);
            Vector3 centerMarkCorner4 = new Vector3(cameraPos.x + 0.1f, cameraPos.y + 0.1f, 0);

            Debug.DrawLine(centerMarkCorner1, centerMarkCorner2, Color.cyan);
            Debug.DrawLine(centerMarkCorner3, centerMarkCorner4, Color.cyan);
        }
    }

    public void ExpandLimits(Bounds newBounds)
    {
        var min = newBounds.min;
        var max = newBounds.max;

        if (limitLeft < min.x)
            min.x = limitLeft;
        if (limitBottom < min.y)
            min.y = limitBottom;
        if (limitRight > max.x)
            max.x = limitRight;
        if (limitTop > max.y)
            max.y = limitTop;

        newBounds.min = min;
        newBounds.max = max;

        SetLimits(newBounds, true);
    }

    public void SetLimits(Bounds newBounds, bool transition)
    {
        CorrectExtents(newBounds);

        if (transition)
        {
            if (limitLeft != newBounds.min.x || limitRight != newBounds.max.x || limitTop != newBounds.max.y || limitBottom != newBounds.min.y)
            {
                StartTweenLimits(newBounds.min.x, newBounds.max.x, newBounds.min.y, newBounds.max.y);
            }
        }
        else
        {
            if(_limitTween != null)
            {
                StopCoroutine(_limitTween);
            }

            limitCameraMovementX = true;
            limitCameraMovementY = true;
            limitLeft = newBounds.min.x;
            limitBottom = newBounds.min.y;
            limitRight = newBounds.max.x;
            limitTop = newBounds.max.y;
        }
    }

    public void CorrectExtents(Bounds newBounds)
    {  
        //ensure newbounds is not smaller than screen size
        var extents = newBounds.extents;
		float halfWidth = this.halfWidth;
        var orthoSize = orthographicSize;

        if (newBounds.extents.y < orthoSize)
        {
            extents.y = orthoSize;
        }

        if (newBounds.extents.x < halfWidth)
        {
            extents.x = halfWidth;
        }

        newBounds.extents = extents;
    }

    public void StartTweenLimits(float leftLimit, float rightLimit, float bottomLimit, float topLimit)
    {
        if (_limitTween != null)
        {
            StopCoroutine(_limitTween);
        }

        _limitTween = TweenLimits(leftLimit, rightLimit, bottomLimit, topLimit);
        StartCoroutine(_limitTween);
    }

    private IEnumerator TweenLimits(float newLimitLeft, float newLimitRight, float newLimitBottom, float newLimitTop)
    {
        _tweening = true;

        yield return new WaitForEndOfFrame();

        limitCameraMovementX = true;
        limitCameraMovementY = true;

        var halfHeight = orthographicSize;
        // Clamp the current limits to the current window (TODO: or new limits)
        limitTop = cameraPosition.y + halfHeight;
        var topEdge = limitTop;
        limitBottom = cameraPosition.y - halfHeight;
        var bottomEdge = limitBottom;

		float halfWidth = this.halfWidth;
        limitRight = cameraPosition.x + halfWidth;
        var rightEdge = limitRight;
        limitLeft = cameraPosition.x - halfWidth;
        var leftEdge = limitLeft;

        bool done = false;

        while (!done)
        {
            done = true;
            float speed;
            if (DeathmatchManager.instance)
            {
                speed = Mathf.Clamp(18 * Time.unscaledDeltaTime, 0, 18f / 30f);
            }
            else
            {
                speed = Mathf.Clamp(24 * Time.unscaledDeltaTime, 0, 24f / 30f);
            }
            
            if(_zooming)
            {
                halfWidth = this.halfWidth;
                halfHeight = orthographicSize;
            }

            if (newLimitLeft != limitLeft)
            {
                done = false;                
                limitLeft = Mathf.MoveTowards(limitLeft, newLimitLeft, speed);
                leftEdge = cameraPosition.x - halfWidth;
                if (leftEdge - 2 > limitLeft && leftEdge >= newLimitLeft && limitRight > limitLeft)
                {
                    limitLeft = newLimitLeft;
                }
            }

            if (newLimitRight != limitRight)
            {
                done = false;
                limitRight = Mathf.MoveTowards(limitRight, newLimitRight, speed);
                rightEdge = cameraPosition.x + halfWidth;
                if(rightEdge + 2 < limitRight && rightEdge <= newLimitRight && limitRight > limitLeft)
                {
                    limitRight = newLimitRight;
                }
            }

            if (newLimitBottom != limitBottom)
            {
                done = false;
                limitBottom = Mathf.MoveTowards(limitBottom, newLimitBottom, speed);
                bottomEdge = cameraPosition.y - halfHeight;
                if (bottomEdge -2 > limitBottom && bottomEdge >= newLimitBottom)
                {
                    limitBottom = newLimitBottom;
                }
            }

            if (newLimitTop != limitTop)
            {
                done = false;
                limitTop = Mathf.MoveTowards(limitTop, newLimitTop, speed);
                topEdge = cameraPosition.y + halfHeight;
                if (topEdge + 2 < limitTop && topEdge <= newLimitTop)
                {
                    limitTop = newLimitTop;
                }
            }

            yield return null;
        }

        _tweening = false;
        ValidateLeftAndRightLimits();
        ValidateTopAndBottomLimits();
    }

    public void ActivateLimits(float leftLimit, float rightLimit, float bottomLimit, float topLimit)
    {
        ActivateLimitsX(leftLimit, rightLimit);
        ActivateLimitsY(bottomLimit, topLimit);
    }

    public void ActivateLimitsX(float leftLimit, float rightLimit)
    {
        limitLeft = leftLimit;
        limitRight = rightLimit;
        ValidateLeftAndRightLimits();
        limitCameraMovementX = true;
    }

    public void ActivateLimitsY(float bottomLimit, float topLimit)
    {
        limitBottom = bottomLimit;
        limitTop = topLimit;
        ValidateTopAndBottomLimits();
        limitCameraMovementY = true;
    }

    public void DeactivateLimits()
    {
        limitCameraMovementX = false;
        limitCameraMovementY = false;
    }

    public void SnapCamera(Vector3 targetPosition)
    {
        targetPosition.z = transform.position.z;
        transform.position = targetPosition;
    }

    public void MoveCamera(Vector3 targetPosition, float moveSpeed)
    {
        StartCoroutine(MoveToPosition(targetPosition, moveSpeed));
    }

    public void ZoomCamera(float zoomAmount, float zoomTime)
    {
        StartCoroutine(LerpOrthographicSize(zoomAmount, zoomTime));
    }

    public IEnumerator MoveToPosition(Vector3 targetPosition, float moveSpeed)
    {
        var origActiveTracking = activeTracking;
        activeTracking = false;

        while (transform.position != targetPosition)
        {
            //recalculate due to zooming
            if (limitCameraMovementY)
            {
                var halfHeight = orthographicSize;
                targetPosition.y = Mathf.Clamp(targetPosition.y, limitBottom + halfHeight, limitTop - halfHeight);
            }

            if (limitCameraMovementX)
            {
                var halfWidth = this.halfWidth;
                targetPosition.x = Mathf.Clamp(targetPosition.x, limitLeft + halfWidth, limitRight - halfWidth);
            }

            targetPosition.z = transform.position.z;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        activeTracking = origActiveTracking;
    }

    public IEnumerator LerpOrthographicSize(float zoomScale, float zoomTime)
    {
        if(!camera)
        {
            Debug.LogError("LerpOrthographicSize called on MainCamera" + gameObject.name + "with no camera assigned!");
            yield break;
        }

        var timer = 0f;
        var originalOrthoGraphicSize = camera.orthographicSize;
        var zoomingIn = originalOrthoGraphicSize > defaultSize * zoomScale;
        _zooming = true;

        yield return new WaitForEndOfFrame();

        if (_pixelPerfect && screenMesh && zoomScale > 1)
        {
            camera.targetTexture = renderT768;
            screenMesh.material = renderMat768;
        }

        while (timer < zoomTime)
        {
            timer += zoomingIn ? Time.unscaledDeltaTime : Time.deltaTime;
            camera.orthographicSize = Mathf.Lerp(originalOrthoGraphicSize, defaultSize * zoomScale, timer);
            yield return null;
        }

        if (_pixelPerfect && screenMesh && zoomScale <= 1)
        {
            camera.targetTexture = renderT384;
            screenMesh.material = renderMat384;
        }

        camera.orthographicSize = defaultSize * zoomScale;
        _zooming = false;
    }

    public void Shake(float time, float magnitude = 0.2f, float speed = 6f)
    {
        if(_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _lastShakeOffset = Vector3.zero;
        }

        _shakeCoroutine = ShakeCamera(time, magnitude, speed);
        StartCoroutine(_shakeCoroutine);
    }

    public IEnumerator ShakeCamera(float time, float magnitude, float speed)
    {
        var timer = 0f;
        while (timer < time)
        {
            var targetOffset = Random.insideUnitSphere * magnitude;
            while (_shakeOffset != targetOffset & timer < time)
            {
                timer += Time.deltaTime;
                _lastShakeOffset = _shakeOffset;
                _shakeOffset = Vector3.MoveTowards(_shakeOffset, targetOffset, speed * Time.deltaTime);
                yield return null;
            }
        }

        while (_shakeOffset != Vector3.zero)
        {
            _lastShakeOffset = _shakeOffset;
            _shakeOffset = Vector3.MoveTowards(_shakeOffset, Vector3.zero, speed * Time.deltaTime);
            yield return null;
        }

        _lastShakeOffset = Vector3.zero;
    }

	public bool OffCamera(Vector3 position, float tolerance = 0, bool xAxis = true, bool yAxis = true)
	{
		var halfWidth = this.halfWidth;
        var halfHeight = orthographicSize;

		if (xAxis && (position.x + tolerance < transform.position.x - halfWidth || 
			position.x + tolerance > transform.position.x + halfWidth))
		{
			return true;
		}
		else if (yAxis && (position.y + tolerance < transform.position.y - halfHeight ||
			position.y - tolerance > transform.position.y + halfHeight))
		{
			return true;
		}
		return false;
	}

    private void OnDestroy()
    {
        instance = null;
    }

    public void OnDrawGizmosSelected()
    {
        if (showDebugBoxes) DrawDebugBox();
    }

    public void SetSpooky()
    {
        _analogGlitch.enabled = true;
        _analogGlitch.scanLineJitter = 0.01f;
        _analogGlitch.colorDrift = 0.02f;
        _analogGlitch.verticalJump = 0.01f;
    }

    public void SetGlitch(bool toggle)
    {
        if (_glitchFading) return;

        _digitalGlitch.enabled = toggle;
        _digitalGlitch.intensity = 0.2f;
        _analogGlitch.enabled = toggle;
        _analogGlitch.scanLineJitter = 0;
        _analogGlitch.verticalJump = 0;
        _analogGlitch.horizontalShake = 0;
        _analogGlitch.colorDrift = 0.1f;

        if(!toggle && SaveGameManager.activeGame != null && SaveGameManager.activeGame.gameMode == GameMode.Spooky)
        {
            SetSpooky();
        }
    }

    public void FadeGlitchIn(float time, float digitalIntensity, float scanlineJitter, float verticalJump, float horizontalShake, float colorDrift)
    {
        StartCoroutine(FadeGlitchInRoutine(time, digitalIntensity, scanlineJitter, verticalJump, horizontalShake, colorDrift));
    }

    public void FadeGlitchOut(float time)
    {
        StartCoroutine(FadeGlitchOutRoutine(time));
    }

    public IEnumerator FadeGlitchInRoutine(float time, float digitalIntensity, float scanlineJitter, float verticalJump, float horizontalShake, float colorDrift)
    {
        var timer = 0f;
        _glitchFading = true;

        _digitalGlitch.enabled = true;
        _digitalGlitch.intensity = 0f;
        _analogGlitch.enabled = true;
        _analogGlitch.scanLineJitter = 0;
        _analogGlitch.verticalJump = 0;
        _analogGlitch.horizontalShake = 0;
        _analogGlitch.colorDrift = 0;

        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            _digitalGlitch.intensity = Mathf.Lerp(0, digitalIntensity, timer/time);
            _analogGlitch.scanLineJitter = Mathf.Lerp(0, scanlineJitter, timer / time);
            _analogGlitch.verticalJump = Mathf.Lerp(0, verticalJump, timer / time);
            _analogGlitch.horizontalShake = Mathf.Lerp(0, horizontalShake, timer / time);
            _analogGlitch.colorDrift = Mathf.Lerp(0, colorDrift, timer / time);
            yield return null;
        }

        _glitchFading = false;

        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.gameMode == GameMode.Spooky)
        {
            SetSpooky();
        }
    }

    public IEnumerator FadeGlitchOutRoutine(float time)
    {
        _glitchFading = true;
        _digitalGlitch.enabled = true;
        var origIntensity = _digitalGlitch.intensity;
        _analogGlitch.enabled = true;
        var origScanline = _analogGlitch.scanLineJitter;
        var origVertical = _analogGlitch.verticalJump;
        var origHorizontal = _analogGlitch.horizontalShake;
        var origColorDrift = _analogGlitch.colorDrift;

        var timer = 0f;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            _digitalGlitch.intensity = Mathf.Lerp(origIntensity, 0, timer / time);
            _analogGlitch.scanLineJitter = Mathf.Lerp(origScanline, 0, timer / time);
            _analogGlitch.verticalJump = Mathf.Lerp(origVertical, 0, timer / time);
            _analogGlitch.horizontalShake = Mathf.Lerp(origHorizontal, 0, timer / time);
            _analogGlitch.colorDrift = Mathf.Lerp(origColorDrift, 0, timer / time);
            yield return null;
        }

        _digitalGlitch.enabled = false;
        _analogGlitch.enabled = false;
        _glitchFading = false;
    }
}
