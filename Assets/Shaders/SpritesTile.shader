// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Tile"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_FlashColor("Flash Color", Color) = (1,1,1,1)
		_FlashAmount("Flash Amount",Range(0.0,1.0)) = 0.0
		_RepeatX("Repeat X", Float) = 1
		_RepeatY("Repeat Y", Float) = 1
		_OffsetX("Offset X", Float) = 0
		_OffsetY("Offset Y", Float) = 0
    }
 
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
 
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Fog { Mode Off }
       
        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
           
            struct appdata_t
            {
                half4 vertex   : POSITION;
                half4 color    : COLOR;
                half2 texcoord : TEXCOORD0;
            };
 
            struct v2f
            {
                half4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };
           
            fixed4 _Color;
			fixed4 _FlashColor;
			float _FlashAmount;
            half _RepeatX;
            half _RepeatY;
			half _OffsetX;
			half _OffsetY;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = (IN.texcoord + half2(_OffsetX / _RepeatX, _OffsetY / _RepeatY)) * half2(_RepeatX, _RepeatY);
                OUT.color = IN.color * _Color;
                return OUT;
            }
 
            sampler2D _MainTex;
           
            fixed4 frag(v2f IN) : COLOR
            {
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashAmount);
				c.rgb *= c.a;

				return c;
            }
        ENDCG
        }
    }
}