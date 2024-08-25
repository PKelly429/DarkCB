Shader "Custom/GridOctagon" { //Terrain Grid
	Properties{
		_GridSpacing("Grid Spacing", Float) = 10.0
		_GridSpacingWorldSpace("Grid Spacing World Space", Float) = 10.0
		_GridOffset("Grid Offset", Float) = 0
		[HDR]_GridColour("Grid Colour", Color) = (0.5, 1.0, 1.0, 1.0)
		[HDR]_GridResourceColour("Grid Resource Colour", Color) = (0.5, 1.0, 1.0, 1.0)
		[HDR]_GridColourInvalid("Grid Colour Invalid", Color) = (0.5, 1.0, 1.0, 1.0)
		_Texcoord("Input Texture", 2D) = "white" {}
		_TexcoordLightArea("Light Area Texture", 2D) = "white" {}
		_TexcoordOctagonOutline("Octagon Outline Texture", 2D) = "white" {}
		_TexcoordOctagonInvalid("Octagon Invalid Texture", 2D) = "white" {}
		_TexcoordOctagonFilled("Octagon Filled Texture", 2D) = "white" {}
		_TexcoordOctagonResource("Octagon Resource Texture", 2D) = "white" {}
		_GridRadialTexture("Texture", 2D) = "white" {}
		_ObjPos("ObjPos", Vector) = (1,1,1,1)
		_Radius("HoleRadius", Range(0.01,200000)) = 2
		_GridBrightness("GridBrightness", Range(0.01,200)) = 1
		_PowerGridBrightness("PowerGridBrightness", Range(0.01,200)) = 1
		_PowerGridActive("PowerGridActive", Range(0,1)) = 0
	}
		SubShader{
			Tags {"RenderType" = "Opaque" "Queue" = "Geometry" "ForceNoShadowCasting" = "True" }
			  Pass {
				Name "GRIDPASS"
				Cull Back
				ZTest LEqual
				ZWrite On
				Offset 0, 0
				Blend SrcAlpha OneMinusSrcAlpha
				CGPROGRAM
			sampler2D _Texcoord;
			sampler2D _TexcoordPower;
			uniform float4 _Texcoord_TexelSize;
			sampler2D _TexcoordOctagonOutline;
			sampler2D _TexcoordOctagonInvalid;
			sampler2D _TexcoordOctagonFilled;
			sampler2D _TexcoordOctagonResource;
			sampler2D _TexcoordLightArea;
			sampler2D _TexcoordAnomaly;
			// Define the vertex and fragment shader functions
			#pragma vertex vert
			#pragma fragment frag
			
			// Access Shaderlab properties
			uniform half _GridSpacing;
			uniform half _GridSpacingWorldSpace;
			uniform half _GridOffset;
			uniform half4  _GridColour;
			uniform half4  _GridResourceColour;
			uniform half4 _GridColourInvalid;
			uniform half4 _GridColourAnomaly;
			uniform float _GridBrightness;
			uniform float _PowerGridBrightness;
			uniform bool _PowerGridActive;
			uniform half _GridType;
			uniform half _GlowShift;
			sampler2D _GridRadialTexture;
			sampler2D _Mask;
			uniform float4 _ObjPos;
			uniform float _Radius;

			// Input into the vertex shader
			struct vertexInput {
				half4 vertex : POSITION;
			};

			// Output from vertex shader into fragment shader
			struct vertexOutput {
				half4 pos : SV_POSITION;
				half4 worldPos : TEXCOORD0;
			  half time: TEXCOORD1;
			};

			// VERTEX SHADER
			  vertexOutput vert(vertexInput input) {
			  vertexOutput output;
			  output.pos = UnityObjectToClipPos(input.vertex);
			  float4 appendResult23 = (float4(0.0, (1.0 - 0), 0.0, 0.0));
			  input.vertex.xyz += appendResult23.xyz;
			  // Calculate the world position coordinates to pass to the fragment shader
			  output.worldPos = mul(unity_ObjectToWorld, input.vertex);
			  output.time = _Time;
			  return output;
			}

			half4 frag(vertexOutput input) : COLOR{

				float dx = length(_ObjPos.x - input.worldPos.x);
				float dy = length(_ObjPos.y - input.worldPos.y);
				float dz = length(_ObjPos.z - input.worldPos.z);
				float dist = ((dx * dx + dy * dy + dz * dz) / _Radius);
				dist = 1 - clamp(dist, 0.05, 1);

				half roundedXPos = (input.worldPos.x / _GridSpacing);
				half roundedZPos = (input.worldPos.z / _GridSpacing);
				half roundedXPos2 = (input.worldPos.x / _GridSpacingWorldSpace);
				half roundedZPos2 = (input.worldPos.z / _GridSpacingWorldSpace);
				half roundedXPos4 = (input.worldPos.x / _GridSpacingWorldSpace);
				half roundedZPos4 = (input.worldPos.z / _GridSpacingWorldSpace);

				half2 pixelPos3 = half2((roundedXPos2) / 512, (roundedZPos2) / 512);
				half4 myBuildingPlacementPixelColor3 = tex2D(_Texcoord, pixelPos3);

				half2 pixelPos4 = half2((roundedXPos) / 200, (roundedZPos) / 200);
				half4 myPixelColorFilledOctagon = tex2D(_TexcoordOctagonFilled, pixelPos4);				
				
				half2 pixelPosResource = half2((roundedXPos) / 200, (roundedZPos) / 200);
				half4 myPixelColorResource = tex2D(_TexcoordOctagonResource, pixelPosResource);

				half2 pixelPosLight = half2((roundedXPos4) / 512, (roundedZPos4) / 512);
				half4 myPixelColorLight = tex2D(_TexcoordLightArea, pixelPosLight);
				myPixelColorLight[3] = 1; // Settings this to one because the alpha is 0 so we can use this in the sub command menu

				half2 pixelPos5 = half2((roundedXPos) / 200, (roundedZPos) / 200);
				half4 myPixelColorInvalidOctagon = tex2D(_TexcoordOctagonInvalid, pixelPos5);
						
				// invalid
				if (myBuildingPlacementPixelColor3[0] >= 0.95 &&
					myBuildingPlacementPixelColor3[1] <= 0 &&
					myBuildingPlacementPixelColor3[2] <= 0 &&
					_PowerGridActive == 0)
				{
					return myPixelColorFilledOctagon * myPixelColorInvalidOctagon * _PowerGridBrightness * _GridColourInvalid * myPixelColorLight;
				}
				//resources
				else if (myBuildingPlacementPixelColor3[0] >= 1 && myBuildingPlacementPixelColor3[1] <= 0.6470588 && _PowerGridActive == 0)
				{
					return myPixelColorResource * myBuildingPlacementPixelColor3 * _GridResourceColour * _GridBrightness * myPixelColorLight;
				}
				// outline
				else 
				{
					return myBuildingPlacementPixelColor3 * _GridColour * _GridBrightness * myPixelColorLight;
				}

				//return myPixelColorOutlineOctagon * myPowerLinePixelColor2 * myBuildingPlacementPixelColor3 * _GridBrightness / dist;
			}
		   ENDCG
		}
		}
}