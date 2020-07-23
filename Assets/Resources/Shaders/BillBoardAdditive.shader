// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/BillBoardAdditive" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_scaleXY("ScaleXY", Vector) = (0.5,0.5,0.5,0.5)
		_Color("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	}

		SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		pass {
			Cull Off
				//ZTest Less
				Ztest Always
				Blend Off
				//Blend OneMinusDstColor One
				//Blend SrcColor One
				//AlphaTest Greater .01
				ColorMask RGB
				Cull Off Lighting Off Fog{ Color(0,0,0,0) }
				ZWrite Off
				CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			struct v2f {
				float4 pos:SV_POSITION;
				float2 texc:TEXCOORD0;
			};

			uniform Vector _scaleXY;
			fixed4 _Color;

			v2f vert(appdata_base v) {
				v2f o;
				float4 ori = mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1));
				float4 vt = v.vertex;

				o.pos = mul(UNITY_MATRIX_P,
					mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + float4(v.vertex.x * _scaleXY.x, v.vertex.y * _scaleXY.x, 0.0, 0.0));

				o.texc = v.texcoord;
				return o;
			}

			float4 frag(v2f i) :COLOR {
				return tex2D(_MainTex,i.texc) * _Color;
			}
			ENDCG
		}
	}
}