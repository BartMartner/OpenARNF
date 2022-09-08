Shader "Sprites/DistortTile" 
{

	Properties 
	{
		_Refraction ("Refraction", Range (0.00, 100.0)) = 1.0
		[PerRendererData] _MainTex ("Base (RGB)", 2D) = "white" {}
		_RepeatX("Repeat X", Float) = 1
		_RepeatY("Repeat Y", Float) = 1
		_OffsetX("Offset X", Float) = 0
		_OffsetY("Offset Y", Float) = 0
	}

	SubShader 
	{	
		Tags
		{
			"Queue" = "Overlay"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		LOD 100
	
		GrabPass{  }
	
		Pass
		{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		sampler2D _GrabTexture;
		sampler2D _MainTex;
		float _Refraction;
		half _RepeatX;
		half _RepeatY;
		half _OffsetX;
		half _OffsetY;

		float4 _GrabTexture_TexelSize;

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			half2 texcoord  : TEXCOORD0;
			half4 grabPos : TEXCOORD1;
		};

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			OUT.vertex = UnityObjectToClipPos(IN.vertex);
			OUT.texcoord = IN.texcoord;
			OUT.color = IN.color;
#ifdef PIXELSNAP_ON
			OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif
			OUT.grabPos = ComputeGrabScreenPos(OUT.vertex);
			return OUT;
		};

		float4 frag(v2f IN) : SV_Target
		{
			float4 c = tex2D(_MainTex, (IN.texcoord + half2(_OffsetX, _OffsetY)) * half2(_RepeatX, _RepeatY)) * IN.color;
			float2 offset = c * _Refraction * _GrabTexture_TexelSize.xy;
			float4 screenPos = IN.grabPos;
			screenPos.xy = IN.grabPos.z * offset + IN.grabPos.xy;
			c = tex2Dproj(_GrabTexture, screenPos);
			return c;
		};
		ENDCG
		}
	}	
}