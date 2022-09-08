Shader "Sprites/DefaultFlashPalette"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Palette("Palette", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_FlashColor("Flash Color", Color) = (1,1,1,1)
		_FlashAmount("Flash Amount",Range(0.0,1.0)) = 0.0
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Mode Off }
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"

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
			};

			fixed4 _Color;
			fixed4 _FlashColor;
			float _FlashAmount;
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
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif
				
				return OUT;
			}

			fixed4 frag(v2f IN) : COLOR
			{
				fixed4 c = GetPaletteColor(IN.texcoord) * IN.color;
				c.rgb = lerp(c.rgb,_FlashColor.rgb,_FlashAmount);
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}

	Fallback "Sprites/DefaultFlash"
}