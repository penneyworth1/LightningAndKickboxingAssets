Shader "SoxShader/Particle-AdditiveDissolve"
{
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		_SoftDissolve("Soft Dissolve", Range(0, 1)) = 0.4
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog{ Mode Off }

		CGPROGRAM
		#pragma surface surf Unlit nofog noforwardadd

		fixed4 LightingUnlit(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * atten;
			c.a = s.Alpha;
			return c;
		}

		sampler2D _MainTex;
		fixed _SoftDissolve;

		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			fixed avr = (c.r + c.g + c.b) * 0.33333;
			fixed invAlpha = 1 - IN.color.a;
			o.Albedo = lerp(0, c.rgb, clamp((avr - invAlpha) / _SoftDissolve, 0, 1)) * IN.color;
			o.Alpha = 1;
		}
		ENDCG
	}

	Fallback "Mobile/VertexLit"
}