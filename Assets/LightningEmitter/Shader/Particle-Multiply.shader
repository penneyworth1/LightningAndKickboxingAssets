// Simplified Multiply Particle shader. Differences from regular Multiply Particle one:
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "SoxShader/Particle-Multiply" {
Properties {
	_MainTex ("Diffuse Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend Zero SrcColor
	Cull Off Lighting Off ZWrite Off Fog { Mode Off }
	
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		Pass {
			SetTexture [_MainTex] {
				combine texture * primary
			}
			SetTexture [_MainTex] {
				constantColor (1,1,1,1)
				combine previous lerp (previous) constant
			}
		}
	}
}
}
