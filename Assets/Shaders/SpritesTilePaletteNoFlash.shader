// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/TilePaletteNoFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Palette("Palette", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
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
		Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Fog { Mode Off }
       
        Pass
        {
        CGPROGRAM

			#pragma target 4.0
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
			sampler2D _MainTex;
			sampler2D _Palette;

			fixed4 GetPaletteColor(float2 uv)
			{
				fixed4 source = tex2D(_MainTex, uv);

				if (source.a == 0)
				{
					return source;
				}

				uint r = ceil(source.r * 255);
				uint g = ceil(source.g * 255);
				uint b = ceil(source.b * 255);
				uint index = 0;

				if (r & 2) index += 32;
				if (r & 1) index += 16;
				if (g & 2) index += 8;
				if (g & 1) index += 4;
				if (b & 2) index += 2;
				if (b & 1) index += 1;

				fixed div8 = index / 8.0;
				fixed4 color = tex2D(_Palette, float2(div8 - floor(div8), div8 / 8.0));
				return color;
			}

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = (IN.texcoord + half2(_OffsetX/_RepeatX, _OffsetY/_RepeatY)) * half2(_RepeatX, _RepeatY);
                OUT.color = IN.color * _Color;
                return OUT;
            }
           
            fixed4 frag(v2f IN) : COLOR
            {
				fixed4 c = GetPaletteColor(IN.texcoord) * IN.color;
				c.rgb *= c.a;

				return c;
            }
        ENDCG
        }
    }
}