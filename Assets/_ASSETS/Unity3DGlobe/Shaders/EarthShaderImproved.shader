Shader "Custom/EarthShaderImproved"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FogColor ("Fog Color", Color) = (1,1,1,1)
		_FogStrength ("Fog Strength", Range(0, 5)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull FRONT
			ZWrite Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _FogColor;
			float _FogStrength;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex+normalize(v.normal)*0.125);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal=normalize( mul( UNITY_MATRIX_IT_MV, v.normal.xyzz).xyz );
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float intensity = pow(0.8 - dot(i.normal, float3(0,0,1)), 12);
				fixed4 fogCol = _FogColor * (intensity * _FogStrength);
				fogCol.a = saturate(fogCol.a);
				col = fogCol;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _FogColor;
			float _FogStrength;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal=normalize( mul( UNITY_MATRIX_IT_MV, v.normal.xyzz).xyz );
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float intensity = pow(1.25 - dot(i.normal, float3(0,0,1)), 3);
				fixed4 add = _FogColor * (intensity * _FogStrength);
				add.a = 0;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col + add;
			}
			ENDCG
		}
	}
}
