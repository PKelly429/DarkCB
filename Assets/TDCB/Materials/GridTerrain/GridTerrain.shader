// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "GridTerrain"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		_GridTex("_GridTex", 2D) = "white" {}
		_ObjectPlacement("ObjectPlacement", 2D) = "white" {}
		_LineThickness1("LineThickness", Range( 0 , 1)) = 0.1
		_GapThickness("GapThickness", Range( 0 , 1)) = 0.1
		_CellAlpha("CellAlpha", Range( 0 , 1)) = 0.2
		_GridAlpha("GridAlpha", Range( 0 , 1)) = 0.5
		_FadeDistance("FadeDistance", Float) = 0
		_InvalidGridColor("InvalidGridColor", Color) = (1,0,0,0)
		_AvailableResourceColor("AvailableResourceColor", Color) = (0.1254902,0.7490196,0.4196078,1)
		_UnavailableResourceColor("UnavailableResourceColor", Color) = (0.9215686,0.2313726,0.3529412,1)
		_CurrentPlacementGridColor("CurrentPlacementGridColor", Color) = (1,0,0,0)
		_DefaultGridColor("DefaultGridColor", Color) = (0.1490196,0.8705882,0.5058824,1)
		_GridSize3("GridSize", Float) = 512
		_MousePos("MousePos", Vector) = (0,0,0,0)
		[Toggle(_SHOWGRID_ON)] _ShowGrid("ShowGrid", Float) = 1
		_Texture0("Texture 0", 2D) = "white" {}
		_ShowStone("ShowStone", Range( 0 , 1)) = 1
		_ShowTrees("ShowTrees", Range( 0 , 1)) = 1
		_Texture1("Texture 0", 2D) = "white" {}
		_Texture2("Texture 0", 2D) = "white" {}
		_BuildingPosition("BuildingPosition", Vector) = (0,0,0,0)
		_BuildingRange("BuildingRange", Float) = 0


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Unlit" }

		Cull Back
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest Always
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140011


			

			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma shader_feature_local _SHOWGRID_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DefaultGridColor;
			float4 _CurrentPlacementGridColor;
			float4 _InvalidGridColor;
			float4 _UnavailableResourceColor;
			float4 _AvailableResourceColor;
			float2 _BuildingPosition;
			float2 _MousePos;
			float _ShowStone;
			float _GridSize3;
			float _BuildingRange;
			float _ShowTrees;
			float _GapThickness;
			float _CellAlpha;
			float _LineThickness1;
			float _GridAlpha;
			float _FadeDistance;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _ObjectPlacement;
			sampler2D _GridTex;
			sampler2D _Texture0;
			sampler2D _Texture1;
			sampler2D _Texture2;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN
				#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
				#endif
				 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 appendResult47 = (float2(WorldPosition.x , WorldPosition.z));
				float2 WorldPos202 = appendResult47;
				float2 _MapUVOffset = float2(0.5,0.5);
				float2 WorldUV147 = ( ( floor( ( WorldPos202 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float ObjPlacementMap152 = tex2D( _ObjectPlacement, WorldUV147 ).r;
				float4 lerpResult161 = lerp( _DefaultGridColor , _CurrentPlacementGridColor , ObjPlacementMap152);
				float4 tex2DNode18 = tex2D( _GridTex, WorldUV147 );
				float BlockedMap156 = tex2DNode18.r;
				float4 lerpResult160 = lerp( lerpResult161 , _InvalidGridColor , BlockedMap156);
				float4 ValidColour254 = lerpResult160;
				float ResourceAvailable359 = tex2DNode18.b;
				float3 lerpResult361 = lerp( _UnavailableResourceColor.rgb , _AvailableResourceColor.rgb , ResourceAvailable359);
				float3 ResourceColour363 = lerpResult361;
				float ResourceMap304 = tex2DNode18.g;
				float ShowStone220 = _ShowStone;
				float Stone208 = ( (( ResourceMap304 >= 0.45 && ResourceMap304 <= 0.55 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowStone220 );
				float GridSize196 = _GridSize3;
				float2 BuildingPos309 = _BuildingPosition;
				float2 BuildingUV343 = ( ( floor( ( BuildingPos309 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float BuildingRange311 = _BuildingRange;
				float ShowTrees253 = _ShowTrees;
				float Tree249 = ( (( ResourceMap304 >= 0.2 && ResourceMap304 <= 0.3 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowTrees253 );
				float ResourceAlpha327 = ( ( distance( ( WorldUV147 * GridSize196 ) , ( BuildingUV343 * GridSize196 ) ) <= floor( ( BuildingRange311 / 4.0 ) ) ? 1.0 : 0.0 ) * max( Tree249 , Stone208 ) );
				float FinalStoneAlpha358 = ( Stone208 * ResourceAlpha327 );
				float4 lerpResult256 = lerp( ValidColour254 , float4( ResourceColour363 , 0.0 ) , FinalStoneAlpha358);
				float FinalTreeAlpha353 = ( Tree249 * ResourceAlpha327 );
				float4 lerpResult214 = lerp( lerpResult256 , float4( ResourceColour363 , 0.0 ) , FinalTreeAlpha353);
				float4 color107 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
				float VisionMap164 = ( 1.0 - tex2DNode18.a );
				float4 lerpResult106 = lerp( lerpResult214 , color107 , min( VisionMap164 , ( 1.0 - ResourceAlpha327 ) ));
				float4 GridColor211 = lerpResult106;
				
				float3 temp_cast_3 = (0.0).xxx;
				float4 appendResult143 = (float4(GridSize196 , GridSize196 , 0.0 , 0.0));
				float2 texCoord138 = IN.ase_texcoord4.xy * appendResult143.xy + float2( 0,0 );
				float2 TileUV182 = texCoord138;
				float3 CrossTexture199 = ( ( ObjPlacementMap152 * saturate( ( BlockedMap156 + VisionMap164 ) ) ) * tex2D( _Texture0, TileUV182 ).rgb );
				float2 temp_cast_5 = (GridSize196).xx;
				float temp_output_121_0 = ( 1.0 - _GapThickness );
				float temp_output_5_0_g12 = temp_output_121_0;
				float temp_output_2_0_g13 = temp_output_5_0_g12;
				float temp_output_3_0_g13 = temp_output_5_0_g12;
				float2 appendResult21_g13 = (float2(temp_output_2_0_g13 , temp_output_3_0_g13));
				float Radius25_g13 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g13 ) ) , abs( temp_output_3_0_g13 ) ) , 1E-05 );
				float2 temp_cast_6 = (0.0).xx;
				float temp_output_30_0_g13 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord4.xy*temp_cast_5 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g13 ) + Radius25_g13 ) , temp_cast_6 ) ) / Radius25_g13 );
				float CellAlpha170 = ( _CellAlpha * max( max( ObjPlacementMap152 , BlockedMap156 ) , ( ShowStone220 * Stone208 ) ) );
				float clampResult119 = clamp( saturate( ( ( 1.0 - temp_output_30_0_g13 ) / fwidth( temp_output_30_0_g13 ) ) ) , 0.0 , CellAlpha170 );
				float2 temp_cast_7 = (GridSize196).xx;
				float temp_output_5_0_g8 = temp_output_121_0;
				float temp_output_2_0_g9 = temp_output_5_0_g8;
				float temp_output_3_0_g9 = temp_output_5_0_g8;
				float2 appendResult21_g9 = (float2(temp_output_2_0_g9 , temp_output_3_0_g9));
				float Radius25_g9 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g9 ) ) , abs( temp_output_3_0_g9 ) ) , 1E-05 );
				float2 temp_cast_8 = (0.0).xx;
				float temp_output_30_0_g9 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord4.xy*temp_cast_7 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g9 ) + Radius25_g9 ) , temp_cast_8 ) ) / Radius25_g9 );
				float2 temp_cast_9 = (GridSize196).xx;
				float temp_output_5_0_g10 = ( temp_output_121_0 - _LineThickness1 );
				float temp_output_2_0_g11 = temp_output_5_0_g10;
				float temp_output_3_0_g11 = temp_output_5_0_g10;
				float2 appendResult21_g11 = (float2(temp_output_2_0_g11 , temp_output_3_0_g11));
				float Radius25_g11 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g11 ) ) , abs( temp_output_3_0_g11 ) ) , 1E-05 );
				float2 temp_cast_10 = (0.0).xx;
				float temp_output_30_0_g11 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord4.xy*temp_cast_9 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g11 ) + Radius25_g11 ) , temp_cast_10 ) ) / Radius25_g11 );
				float Grid116 = (0.0 + (saturate( ( clampResult119 + saturate( ( saturate( ( ( 1.0 - temp_output_30_0_g9 ) / fwidth( temp_output_30_0_g9 ) ) ) - saturate( ( ( 1.0 - temp_output_30_0_g11 ) / fwidth( temp_output_30_0_g11 ) ) ) ) ) ) ) - 0.0) * (max( ObjPlacementMap152 , ( CellAlpha170 + _GridAlpha ) ) - 0.0) / (1.0 - 0.0));
				float3 StoneTexture233 = ( FinalStoneAlpha358 * tex2D( _Texture1, TileUV182 ).rgb );
				float3 TreeTexture268 = ( FinalTreeAlpha353 * tex2D( _Texture2, TileUV182 ).rgb );
				float smoothstepResult87 = smoothstep( 0.0 , 1.0 , ( 1.0 - saturate( ( distance( WorldPos202 , _MousePos ) / _FadeDistance ) ) ));
				float DistanceAlpha312 = smoothstepResult87;
				#ifdef _SHOWGRID_ON
				float3 staticSwitch89 = ( ( CrossTexture199 + Grid116 + StoneTexture233 + TreeTexture268 ) * max( DistanceAlpha312 , ResourceAlpha327 ) );
				#else
				float3 staticSwitch89 = temp_cast_3;
				#endif
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = GridColor211.rgb;
				float Alpha = staticSwitch89.x;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.positionCS, Color);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma shader_feature_local _SHOWGRID_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DefaultGridColor;
			float4 _CurrentPlacementGridColor;
			float4 _InvalidGridColor;
			float4 _UnavailableResourceColor;
			float4 _AvailableResourceColor;
			float2 _BuildingPosition;
			float2 _MousePos;
			float _ShowStone;
			float _GridSize3;
			float _BuildingRange;
			float _ShowTrees;
			float _GapThickness;
			float _CellAlpha;
			float _LineThickness1;
			float _GridAlpha;
			float _FadeDistance;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _ObjectPlacement;
			sampler2D _GridTex;
			sampler2D _Texture0;
			sampler2D _Texture1;
			sampler2D _Texture2;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float3 temp_cast_0 = (0.0).xxx;
				float2 appendResult47 = (float2(WorldPosition.x , WorldPosition.z));
				float2 WorldPos202 = appendResult47;
				float2 _MapUVOffset = float2(0.5,0.5);
				float2 WorldUV147 = ( ( floor( ( WorldPos202 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float ObjPlacementMap152 = tex2D( _ObjectPlacement, WorldUV147 ).r;
				float4 tex2DNode18 = tex2D( _GridTex, WorldUV147 );
				float BlockedMap156 = tex2DNode18.r;
				float VisionMap164 = ( 1.0 - tex2DNode18.a );
				float GridSize196 = _GridSize3;
				float4 appendResult143 = (float4(GridSize196 , GridSize196 , 0.0 , 0.0));
				float2 texCoord138 = IN.ase_texcoord3.xy * appendResult143.xy + float2( 0,0 );
				float2 TileUV182 = texCoord138;
				float3 CrossTexture199 = ( ( ObjPlacementMap152 * saturate( ( BlockedMap156 + VisionMap164 ) ) ) * tex2D( _Texture0, TileUV182 ).rgb );
				float2 temp_cast_2 = (GridSize196).xx;
				float temp_output_121_0 = ( 1.0 - _GapThickness );
				float temp_output_5_0_g12 = temp_output_121_0;
				float temp_output_2_0_g13 = temp_output_5_0_g12;
				float temp_output_3_0_g13 = temp_output_5_0_g12;
				float2 appendResult21_g13 = (float2(temp_output_2_0_g13 , temp_output_3_0_g13));
				float Radius25_g13 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g13 ) ) , abs( temp_output_3_0_g13 ) ) , 1E-05 );
				float2 temp_cast_3 = (0.0).xx;
				float temp_output_30_0_g13 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord3.xy*temp_cast_2 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g13 ) + Radius25_g13 ) , temp_cast_3 ) ) / Radius25_g13 );
				float ShowStone220 = _ShowStone;
				float ResourceMap304 = tex2DNode18.g;
				float Stone208 = ( (( ResourceMap304 >= 0.45 && ResourceMap304 <= 0.55 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowStone220 );
				float CellAlpha170 = ( _CellAlpha * max( max( ObjPlacementMap152 , BlockedMap156 ) , ( ShowStone220 * Stone208 ) ) );
				float clampResult119 = clamp( saturate( ( ( 1.0 - temp_output_30_0_g13 ) / fwidth( temp_output_30_0_g13 ) ) ) , 0.0 , CellAlpha170 );
				float2 temp_cast_4 = (GridSize196).xx;
				float temp_output_5_0_g8 = temp_output_121_0;
				float temp_output_2_0_g9 = temp_output_5_0_g8;
				float temp_output_3_0_g9 = temp_output_5_0_g8;
				float2 appendResult21_g9 = (float2(temp_output_2_0_g9 , temp_output_3_0_g9));
				float Radius25_g9 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g9 ) ) , abs( temp_output_3_0_g9 ) ) , 1E-05 );
				float2 temp_cast_5 = (0.0).xx;
				float temp_output_30_0_g9 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord3.xy*temp_cast_4 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g9 ) + Radius25_g9 ) , temp_cast_5 ) ) / Radius25_g9 );
				float2 temp_cast_6 = (GridSize196).xx;
				float temp_output_5_0_g10 = ( temp_output_121_0 - _LineThickness1 );
				float temp_output_2_0_g11 = temp_output_5_0_g10;
				float temp_output_3_0_g11 = temp_output_5_0_g10;
				float2 appendResult21_g11 = (float2(temp_output_2_0_g11 , temp_output_3_0_g11));
				float Radius25_g11 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g11 ) ) , abs( temp_output_3_0_g11 ) ) , 1E-05 );
				float2 temp_cast_7 = (0.0).xx;
				float temp_output_30_0_g11 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord3.xy*temp_cast_6 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g11 ) + Radius25_g11 ) , temp_cast_7 ) ) / Radius25_g11 );
				float Grid116 = (0.0 + (saturate( ( clampResult119 + saturate( ( saturate( ( ( 1.0 - temp_output_30_0_g9 ) / fwidth( temp_output_30_0_g9 ) ) ) - saturate( ( ( 1.0 - temp_output_30_0_g11 ) / fwidth( temp_output_30_0_g11 ) ) ) ) ) ) ) - 0.0) * (max( ObjPlacementMap152 , ( CellAlpha170 + _GridAlpha ) ) - 0.0) / (1.0 - 0.0));
				float2 BuildingPos309 = _BuildingPosition;
				float2 BuildingUV343 = ( ( floor( ( BuildingPos309 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float BuildingRange311 = _BuildingRange;
				float ShowTrees253 = _ShowTrees;
				float Tree249 = ( (( ResourceMap304 >= 0.2 && ResourceMap304 <= 0.3 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowTrees253 );
				float ResourceAlpha327 = ( ( distance( ( WorldUV147 * GridSize196 ) , ( BuildingUV343 * GridSize196 ) ) <= floor( ( BuildingRange311 / 4.0 ) ) ? 1.0 : 0.0 ) * max( Tree249 , Stone208 ) );
				float FinalStoneAlpha358 = ( Stone208 * ResourceAlpha327 );
				float3 StoneTexture233 = ( FinalStoneAlpha358 * tex2D( _Texture1, TileUV182 ).rgb );
				float FinalTreeAlpha353 = ( Tree249 * ResourceAlpha327 );
				float3 TreeTexture268 = ( FinalTreeAlpha353 * tex2D( _Texture2, TileUV182 ).rgb );
				float smoothstepResult87 = smoothstep( 0.0 , 1.0 , ( 1.0 - saturate( ( distance( WorldPos202 , _MousePos ) / _FadeDistance ) ) ));
				float DistanceAlpha312 = smoothstepResult87;
				#ifdef _SHOWGRID_ON
				float3 staticSwitch89 = ( ( CrossTexture199 + Grid116 + StoneTexture233 + TreeTexture268 ) * max( DistanceAlpha312 , ResourceAlpha327 ) );
				#else
				float3 staticSwitch89 = temp_cast_0;
				#endif
				

				float Alpha = staticSwitch89.x;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#pragma shader_feature_local _SHOWGRID_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DefaultGridColor;
			float4 _CurrentPlacementGridColor;
			float4 _InvalidGridColor;
			float4 _UnavailableResourceColor;
			float4 _AvailableResourceColor;
			float2 _BuildingPosition;
			float2 _MousePos;
			float _ShowStone;
			float _GridSize3;
			float _BuildingRange;
			float _ShowTrees;
			float _GapThickness;
			float _CellAlpha;
			float _LineThickness1;
			float _GridAlpha;
			float _FadeDistance;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _ObjectPlacement;
			sampler2D _GridTex;
			sampler2D _Texture0;
			sampler2D _Texture1;
			sampler2D _Texture2;


			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				o.ase_texcoord.xyz = ase_worldPos;
				
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.w = 0;
				o.ase_texcoord1.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );

				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float3 temp_cast_0 = (0.0).xxx;
				float3 ase_worldPos = IN.ase_texcoord.xyz;
				float2 appendResult47 = (float2(ase_worldPos.x , ase_worldPos.z));
				float2 WorldPos202 = appendResult47;
				float2 _MapUVOffset = float2(0.5,0.5);
				float2 WorldUV147 = ( ( floor( ( WorldPos202 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float ObjPlacementMap152 = tex2D( _ObjectPlacement, WorldUV147 ).r;
				float4 tex2DNode18 = tex2D( _GridTex, WorldUV147 );
				float BlockedMap156 = tex2DNode18.r;
				float VisionMap164 = ( 1.0 - tex2DNode18.a );
				float GridSize196 = _GridSize3;
				float4 appendResult143 = (float4(GridSize196 , GridSize196 , 0.0 , 0.0));
				float2 texCoord138 = IN.ase_texcoord1.xy * appendResult143.xy + float2( 0,0 );
				float2 TileUV182 = texCoord138;
				float3 CrossTexture199 = ( ( ObjPlacementMap152 * saturate( ( BlockedMap156 + VisionMap164 ) ) ) * tex2D( _Texture0, TileUV182 ).rgb );
				float2 temp_cast_2 = (GridSize196).xx;
				float temp_output_121_0 = ( 1.0 - _GapThickness );
				float temp_output_5_0_g12 = temp_output_121_0;
				float temp_output_2_0_g13 = temp_output_5_0_g12;
				float temp_output_3_0_g13 = temp_output_5_0_g12;
				float2 appendResult21_g13 = (float2(temp_output_2_0_g13 , temp_output_3_0_g13));
				float Radius25_g13 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g13 ) ) , abs( temp_output_3_0_g13 ) ) , 1E-05 );
				float2 temp_cast_3 = (0.0).xx;
				float temp_output_30_0_g13 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord1.xy*temp_cast_2 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g13 ) + Radius25_g13 ) , temp_cast_3 ) ) / Radius25_g13 );
				float ShowStone220 = _ShowStone;
				float ResourceMap304 = tex2DNode18.g;
				float Stone208 = ( (( ResourceMap304 >= 0.45 && ResourceMap304 <= 0.55 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowStone220 );
				float CellAlpha170 = ( _CellAlpha * max( max( ObjPlacementMap152 , BlockedMap156 ) , ( ShowStone220 * Stone208 ) ) );
				float clampResult119 = clamp( saturate( ( ( 1.0 - temp_output_30_0_g13 ) / fwidth( temp_output_30_0_g13 ) ) ) , 0.0 , CellAlpha170 );
				float2 temp_cast_4 = (GridSize196).xx;
				float temp_output_5_0_g8 = temp_output_121_0;
				float temp_output_2_0_g9 = temp_output_5_0_g8;
				float temp_output_3_0_g9 = temp_output_5_0_g8;
				float2 appendResult21_g9 = (float2(temp_output_2_0_g9 , temp_output_3_0_g9));
				float Radius25_g9 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g9 ) ) , abs( temp_output_3_0_g9 ) ) , 1E-05 );
				float2 temp_cast_5 = (0.0).xx;
				float temp_output_30_0_g9 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord1.xy*temp_cast_4 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g9 ) + Radius25_g9 ) , temp_cast_5 ) ) / Radius25_g9 );
				float2 temp_cast_6 = (GridSize196).xx;
				float temp_output_5_0_g10 = ( temp_output_121_0 - _LineThickness1 );
				float temp_output_2_0_g11 = temp_output_5_0_g10;
				float temp_output_3_0_g11 = temp_output_5_0_g10;
				float2 appendResult21_g11 = (float2(temp_output_2_0_g11 , temp_output_3_0_g11));
				float Radius25_g11 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g11 ) ) , abs( temp_output_3_0_g11 ) ) , 1E-05 );
				float2 temp_cast_7 = (0.0).xx;
				float temp_output_30_0_g11 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord1.xy*temp_cast_6 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g11 ) + Radius25_g11 ) , temp_cast_7 ) ) / Radius25_g11 );
				float Grid116 = (0.0 + (saturate( ( clampResult119 + saturate( ( saturate( ( ( 1.0 - temp_output_30_0_g9 ) / fwidth( temp_output_30_0_g9 ) ) ) - saturate( ( ( 1.0 - temp_output_30_0_g11 ) / fwidth( temp_output_30_0_g11 ) ) ) ) ) ) ) - 0.0) * (max( ObjPlacementMap152 , ( CellAlpha170 + _GridAlpha ) ) - 0.0) / (1.0 - 0.0));
				float2 BuildingPos309 = _BuildingPosition;
				float2 BuildingUV343 = ( ( floor( ( BuildingPos309 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float BuildingRange311 = _BuildingRange;
				float ShowTrees253 = _ShowTrees;
				float Tree249 = ( (( ResourceMap304 >= 0.2 && ResourceMap304 <= 0.3 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowTrees253 );
				float ResourceAlpha327 = ( ( distance( ( WorldUV147 * GridSize196 ) , ( BuildingUV343 * GridSize196 ) ) <= floor( ( BuildingRange311 / 4.0 ) ) ? 1.0 : 0.0 ) * max( Tree249 , Stone208 ) );
				float FinalStoneAlpha358 = ( Stone208 * ResourceAlpha327 );
				float3 StoneTexture233 = ( FinalStoneAlpha358 * tex2D( _Texture1, TileUV182 ).rgb );
				float FinalTreeAlpha353 = ( Tree249 * ResourceAlpha327 );
				float3 TreeTexture268 = ( FinalTreeAlpha353 * tex2D( _Texture2, TileUV182 ).rgb );
				float smoothstepResult87 = smoothstep( 0.0 , 1.0 , ( 1.0 - saturate( ( distance( WorldPos202 , _MousePos ) / _FadeDistance ) ) ));
				float DistanceAlpha312 = smoothstepResult87;
				#ifdef _SHOWGRID_ON
				float3 staticSwitch89 = ( ( CrossTexture199 + Grid116 + StoneTexture233 + TreeTexture268 ) * max( DistanceAlpha312 , ResourceAlpha327 ) );
				#else
				float3 staticSwitch89 = temp_cast_0;
				#endif
				

				surfaceDescription.Alpha = staticSwitch89.x;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#pragma shader_feature_local _SHOWGRID_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DefaultGridColor;
			float4 _CurrentPlacementGridColor;
			float4 _InvalidGridColor;
			float4 _UnavailableResourceColor;
			float4 _AvailableResourceColor;
			float2 _BuildingPosition;
			float2 _MousePos;
			float _ShowStone;
			float _GridSize3;
			float _BuildingRange;
			float _ShowTrees;
			float _GapThickness;
			float _CellAlpha;
			float _LineThickness1;
			float _GridAlpha;
			float _FadeDistance;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _ObjectPlacement;
			sampler2D _GridTex;
			sampler2D _Texture0;
			sampler2D _Texture1;
			sampler2D _Texture2;


			
			float4 _SelectionID;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				o.ase_texcoord.xyz = ase_worldPos;
				
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.w = 0;
				o.ase_texcoord1.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
				o.positionCS = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float3 temp_cast_0 = (0.0).xxx;
				float3 ase_worldPos = IN.ase_texcoord.xyz;
				float2 appendResult47 = (float2(ase_worldPos.x , ase_worldPos.z));
				float2 WorldPos202 = appendResult47;
				float2 _MapUVOffset = float2(0.5,0.5);
				float2 WorldUV147 = ( ( floor( ( WorldPos202 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float ObjPlacementMap152 = tex2D( _ObjectPlacement, WorldUV147 ).r;
				float4 tex2DNode18 = tex2D( _GridTex, WorldUV147 );
				float BlockedMap156 = tex2DNode18.r;
				float VisionMap164 = ( 1.0 - tex2DNode18.a );
				float GridSize196 = _GridSize3;
				float4 appendResult143 = (float4(GridSize196 , GridSize196 , 0.0 , 0.0));
				float2 texCoord138 = IN.ase_texcoord1.xy * appendResult143.xy + float2( 0,0 );
				float2 TileUV182 = texCoord138;
				float3 CrossTexture199 = ( ( ObjPlacementMap152 * saturate( ( BlockedMap156 + VisionMap164 ) ) ) * tex2D( _Texture0, TileUV182 ).rgb );
				float2 temp_cast_2 = (GridSize196).xx;
				float temp_output_121_0 = ( 1.0 - _GapThickness );
				float temp_output_5_0_g12 = temp_output_121_0;
				float temp_output_2_0_g13 = temp_output_5_0_g12;
				float temp_output_3_0_g13 = temp_output_5_0_g12;
				float2 appendResult21_g13 = (float2(temp_output_2_0_g13 , temp_output_3_0_g13));
				float Radius25_g13 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g13 ) ) , abs( temp_output_3_0_g13 ) ) , 1E-05 );
				float2 temp_cast_3 = (0.0).xx;
				float temp_output_30_0_g13 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord1.xy*temp_cast_2 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g13 ) + Radius25_g13 ) , temp_cast_3 ) ) / Radius25_g13 );
				float ShowStone220 = _ShowStone;
				float ResourceMap304 = tex2DNode18.g;
				float Stone208 = ( (( ResourceMap304 >= 0.45 && ResourceMap304 <= 0.55 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowStone220 );
				float CellAlpha170 = ( _CellAlpha * max( max( ObjPlacementMap152 , BlockedMap156 ) , ( ShowStone220 * Stone208 ) ) );
				float clampResult119 = clamp( saturate( ( ( 1.0 - temp_output_30_0_g13 ) / fwidth( temp_output_30_0_g13 ) ) ) , 0.0 , CellAlpha170 );
				float2 temp_cast_4 = (GridSize196).xx;
				float temp_output_5_0_g8 = temp_output_121_0;
				float temp_output_2_0_g9 = temp_output_5_0_g8;
				float temp_output_3_0_g9 = temp_output_5_0_g8;
				float2 appendResult21_g9 = (float2(temp_output_2_0_g9 , temp_output_3_0_g9));
				float Radius25_g9 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g9 ) ) , abs( temp_output_3_0_g9 ) ) , 1E-05 );
				float2 temp_cast_5 = (0.0).xx;
				float temp_output_30_0_g9 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord1.xy*temp_cast_4 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g9 ) + Radius25_g9 ) , temp_cast_5 ) ) / Radius25_g9 );
				float2 temp_cast_6 = (GridSize196).xx;
				float temp_output_5_0_g10 = ( temp_output_121_0 - _LineThickness1 );
				float temp_output_2_0_g11 = temp_output_5_0_g10;
				float temp_output_3_0_g11 = temp_output_5_0_g10;
				float2 appendResult21_g11 = (float2(temp_output_2_0_g11 , temp_output_3_0_g11));
				float Radius25_g11 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g11 ) ) , abs( temp_output_3_0_g11 ) ) , 1E-05 );
				float2 temp_cast_7 = (0.0).xx;
				float temp_output_30_0_g11 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord1.xy*temp_cast_6 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g11 ) + Radius25_g11 ) , temp_cast_7 ) ) / Radius25_g11 );
				float Grid116 = (0.0 + (saturate( ( clampResult119 + saturate( ( saturate( ( ( 1.0 - temp_output_30_0_g9 ) / fwidth( temp_output_30_0_g9 ) ) ) - saturate( ( ( 1.0 - temp_output_30_0_g11 ) / fwidth( temp_output_30_0_g11 ) ) ) ) ) ) ) - 0.0) * (max( ObjPlacementMap152 , ( CellAlpha170 + _GridAlpha ) ) - 0.0) / (1.0 - 0.0));
				float2 BuildingPos309 = _BuildingPosition;
				float2 BuildingUV343 = ( ( floor( ( BuildingPos309 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float BuildingRange311 = _BuildingRange;
				float ShowTrees253 = _ShowTrees;
				float Tree249 = ( (( ResourceMap304 >= 0.2 && ResourceMap304 <= 0.3 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowTrees253 );
				float ResourceAlpha327 = ( ( distance( ( WorldUV147 * GridSize196 ) , ( BuildingUV343 * GridSize196 ) ) <= floor( ( BuildingRange311 / 4.0 ) ) ? 1.0 : 0.0 ) * max( Tree249 , Stone208 ) );
				float FinalStoneAlpha358 = ( Stone208 * ResourceAlpha327 );
				float3 StoneTexture233 = ( FinalStoneAlpha358 * tex2D( _Texture1, TileUV182 ).rgb );
				float FinalTreeAlpha353 = ( Tree249 * ResourceAlpha327 );
				float3 TreeTexture268 = ( FinalTreeAlpha353 * tex2D( _Texture2, TileUV182 ).rgb );
				float smoothstepResult87 = smoothstep( 0.0 , 1.0 , ( 1.0 - saturate( ( distance( WorldPos202 , _MousePos ) / _FadeDistance ) ) ));
				float DistanceAlpha312 = smoothstepResult87;
				#ifdef _SHOWGRID_ON
				float3 staticSwitch89 = ( ( CrossTexture199 + Grid116 + StoneTexture233 + TreeTexture268 ) * max( DistanceAlpha312 , ResourceAlpha327 ) );
				#else
				float3 staticSwitch89 = temp_cast_0;
				#endif
				

				surfaceDescription.Alpha = staticSwitch89.x;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			

        	#define _SURFACE_TYPE_TRANSPARENT 1
        	#define ASE_SRP_VERSION 140011


			

        	#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#pragma shader_feature_local _SHOWGRID_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float3 normalWS : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DefaultGridColor;
			float4 _CurrentPlacementGridColor;
			float4 _InvalidGridColor;
			float4 _UnavailableResourceColor;
			float4 _AvailableResourceColor;
			float2 _BuildingPosition;
			float2 _MousePos;
			float _ShowStone;
			float _GridSize3;
			float _BuildingRange;
			float _ShowTrees;
			float _GapThickness;
			float _CellAlpha;
			float _LineThickness1;
			float _GridAlpha;
			float _FadeDistance;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _ObjectPlacement;
			sampler2D _GridTex;
			sampler2D _Texture0;
			sampler2D _Texture1;
			sampler2D _Texture2;


			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				o.ase_texcoord2.xyz = ase_worldPos;
				
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				o.normalWS = TransformObjectToWorldNormal( v.normalOS );
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			void frag( VertexOutput IN
				, out half4 outNormalWS : SV_Target0
			#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
			#endif
				 )
			{
				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				float3 temp_cast_0 = (0.0).xxx;
				float3 ase_worldPos = IN.ase_texcoord2.xyz;
				float2 appendResult47 = (float2(ase_worldPos.x , ase_worldPos.z));
				float2 WorldPos202 = appendResult47;
				float2 _MapUVOffset = float2(0.5,0.5);
				float2 WorldUV147 = ( ( floor( ( WorldPos202 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float ObjPlacementMap152 = tex2D( _ObjectPlacement, WorldUV147 ).r;
				float4 tex2DNode18 = tex2D( _GridTex, WorldUV147 );
				float BlockedMap156 = tex2DNode18.r;
				float VisionMap164 = ( 1.0 - tex2DNode18.a );
				float GridSize196 = _GridSize3;
				float4 appendResult143 = (float4(GridSize196 , GridSize196 , 0.0 , 0.0));
				float2 texCoord138 = IN.ase_texcoord3.xy * appendResult143.xy + float2( 0,0 );
				float2 TileUV182 = texCoord138;
				float3 CrossTexture199 = ( ( ObjPlacementMap152 * saturate( ( BlockedMap156 + VisionMap164 ) ) ) * tex2D( _Texture0, TileUV182 ).rgb );
				float2 temp_cast_2 = (GridSize196).xx;
				float temp_output_121_0 = ( 1.0 - _GapThickness );
				float temp_output_5_0_g12 = temp_output_121_0;
				float temp_output_2_0_g13 = temp_output_5_0_g12;
				float temp_output_3_0_g13 = temp_output_5_0_g12;
				float2 appendResult21_g13 = (float2(temp_output_2_0_g13 , temp_output_3_0_g13));
				float Radius25_g13 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g13 ) ) , abs( temp_output_3_0_g13 ) ) , 1E-05 );
				float2 temp_cast_3 = (0.0).xx;
				float temp_output_30_0_g13 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord3.xy*temp_cast_2 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g13 ) + Radius25_g13 ) , temp_cast_3 ) ) / Radius25_g13 );
				float ShowStone220 = _ShowStone;
				float ResourceMap304 = tex2DNode18.g;
				float Stone208 = ( (( ResourceMap304 >= 0.45 && ResourceMap304 <= 0.55 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowStone220 );
				float CellAlpha170 = ( _CellAlpha * max( max( ObjPlacementMap152 , BlockedMap156 ) , ( ShowStone220 * Stone208 ) ) );
				float clampResult119 = clamp( saturate( ( ( 1.0 - temp_output_30_0_g13 ) / fwidth( temp_output_30_0_g13 ) ) ) , 0.0 , CellAlpha170 );
				float2 temp_cast_4 = (GridSize196).xx;
				float temp_output_5_0_g8 = temp_output_121_0;
				float temp_output_2_0_g9 = temp_output_5_0_g8;
				float temp_output_3_0_g9 = temp_output_5_0_g8;
				float2 appendResult21_g9 = (float2(temp_output_2_0_g9 , temp_output_3_0_g9));
				float Radius25_g9 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g9 ) ) , abs( temp_output_3_0_g9 ) ) , 1E-05 );
				float2 temp_cast_5 = (0.0).xx;
				float temp_output_30_0_g9 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord3.xy*temp_cast_4 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g9 ) + Radius25_g9 ) , temp_cast_5 ) ) / Radius25_g9 );
				float2 temp_cast_6 = (GridSize196).xx;
				float temp_output_5_0_g10 = ( temp_output_121_0 - _LineThickness1 );
				float temp_output_2_0_g11 = temp_output_5_0_g10;
				float temp_output_3_0_g11 = temp_output_5_0_g10;
				float2 appendResult21_g11 = (float2(temp_output_2_0_g11 , temp_output_3_0_g11));
				float Radius25_g11 = max( min( min( abs( ( 0.2 * 2 ) ) , abs( temp_output_2_0_g11 ) ) , abs( temp_output_3_0_g11 ) ) , 1E-05 );
				float2 temp_cast_7 = (0.0).xx;
				float temp_output_30_0_g11 = ( length( max( ( ( abs( (frac( (IN.ase_texcoord3.xy*temp_cast_6 + float2( 0,0 )) )*2.0 + -1.0) ) - appendResult21_g11 ) + Radius25_g11 ) , temp_cast_7 ) ) / Radius25_g11 );
				float Grid116 = (0.0 + (saturate( ( clampResult119 + saturate( ( saturate( ( ( 1.0 - temp_output_30_0_g9 ) / fwidth( temp_output_30_0_g9 ) ) ) - saturate( ( ( 1.0 - temp_output_30_0_g11 ) / fwidth( temp_output_30_0_g11 ) ) ) ) ) ) ) - 0.0) * (max( ObjPlacementMap152 , ( CellAlpha170 + _GridAlpha ) ) - 0.0) / (1.0 - 0.0));
				float2 BuildingPos309 = _BuildingPosition;
				float2 BuildingUV343 = ( ( floor( ( BuildingPos309 / 4.0 ) ) / 256.0 ) + _MapUVOffset );
				float BuildingRange311 = _BuildingRange;
				float ShowTrees253 = _ShowTrees;
				float Tree249 = ( (( ResourceMap304 >= 0.2 && ResourceMap304 <= 0.3 ) ? 1.0 :  0.0 ) * ( 1.0 - ObjPlacementMap152 ) * ShowTrees253 );
				float ResourceAlpha327 = ( ( distance( ( WorldUV147 * GridSize196 ) , ( BuildingUV343 * GridSize196 ) ) <= floor( ( BuildingRange311 / 4.0 ) ) ? 1.0 : 0.0 ) * max( Tree249 , Stone208 ) );
				float FinalStoneAlpha358 = ( Stone208 * ResourceAlpha327 );
				float3 StoneTexture233 = ( FinalStoneAlpha358 * tex2D( _Texture1, TileUV182 ).rgb );
				float FinalTreeAlpha353 = ( Tree249 * ResourceAlpha327 );
				float3 TreeTexture268 = ( FinalTreeAlpha353 * tex2D( _Texture2, TileUV182 ).rgb );
				float smoothstepResult87 = smoothstep( 0.0 , 1.0 , ( 1.0 - saturate( ( distance( WorldPos202 , _MousePos ) / _FadeDistance ) ) ));
				float DistanceAlpha312 = smoothstepResult87;
				#ifdef _SHOWGRID_ON
				float3 staticSwitch89 = ( ( CrossTexture199 + Grid116 + StoneTexture233 + TreeTexture268 ) * max( DistanceAlpha312 , ResourceAlpha327 ) );
				#else
				float3 staticSwitch89 = temp_cast_0;
				#endif
				

				float Alpha = staticSwitch89.x;
				float AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float3 normalWS = normalize(IN.normalWS);
					float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on some platforms
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					float3 normalWS = IN.normalWS;
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
				#endif
			}

			ENDHLSL
		}

	
	}
	
	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19603
Node;AmplifyShaderEditor.WorldPosInputsNode;46;-80,1056;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;47;112,1088;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;202;288,1088;Inherit;False;WorldPos;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;203;-3840,-128;Inherit;False;202;WorldPos;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-3728,16;Inherit;False;Constant;_WorldScale;WorldScale;9;0;Create;True;0;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;103;-3568,-128;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-3520,16;Inherit;False;Constant;_MapSize;MapSize;9;0;Create;True;0;0;0;False;0;False;256;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;105;-3456,-128;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;94;-3312,32;Inherit;False;Constant;_MapUVOffset;MapUVOffset;9;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;92;-3312,-128;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;93;-3088,-48;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;-2960,-48;Inherit;False;WorldUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;308;-2672.109,-1455.074;Inherit;False;Property;_BuildingPosition;BuildingPosition;20;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;148;-4544,560;Inherit;False;147;WorldUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;149;-4576,352;Inherit;True;Property;_ObjectPlacement;ObjectPlacement;1;0;Create;True;0;0;0;False;0;False;None;e0b7f374567110d499eb79508ab45553;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;204;-2144,-32;Inherit;False;147;WorldUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;17;-2176,-240;Inherit;True;Property;_GridTex;_GridTex;0;0;Create;True;0;0;0;False;0;False;None;82a26e49d2f2e324a97bac033f22300e;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;309;-2480,-1456;Inherit;False;BuildingPos;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;145;-4272,528;Inherit;True;Property;_TextureSample2;Texture Sample 2;12;0;Create;True;0;0;0;False;0;False;-1;None;e0b7f374567110d499eb79508ab45553;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;18;-1840,-160;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;342;-3888,176;Inherit;False;309;BuildingPos;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;152;-3968,576;Inherit;False;ObjPlacementMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;336;-3584,176;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;262;-3344,-1488;Inherit;False;Property;_ShowStone;ShowStone;16;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;304;-1504,-128;Inherit;False;ResourceMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;305;-2240,1248;Inherit;False;304;ResourceMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;338;-3472,176;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;220;-3040,-1488;Inherit;False;ShowStone;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;243;-1680,1632;Inherit;False;152;ObjPlacementMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCCompareWithRange;307;-1952,1440;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.45;False;2;FLOAT;0.55;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;340;-3328,176;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;263;-3328,-1312;Inherit;False;Property;_ShowTrees;ShowTrees;17;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;245;-1472,1632;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;355;-1504,1712;Inherit;False;220;ShowStone;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;310;-2672,-1328;Inherit;False;Property;_BuildingRange;BuildingRange;21;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-3200,-1120;Inherit;False;Property;_GridSize3;GridSize;12;0;Create;True;0;0;0;False;0;False;512;256;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;341;-3104,256;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;253;-3024,-1312;Inherit;False;ShowTrees;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;284;-1696,1328;Inherit;False;152;ObjPlacementMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;244;-1280,1568;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;311;-2480,-1328;Inherit;False;BuildingRange;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;156;-1504,-208;Inherit;False;BlockedMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;196;-3040,-1120;Inherit;False;GridSize;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCCompareWithRange;306;-1952,1248;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;0.3;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;343;-2944,256;Inherit;False;BuildingUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;285;-1488,1328;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;208;-1120,1568;Inherit;False;Stone;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;354;-1520,1408;Inherit;False;253;ShowTrees;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;320;-16,1808;Inherit;False;311;BuildingRange;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;216;-3984,-320;Inherit;False;208;Stone;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;175;-3808,-608;Inherit;False;152;ObjPlacementMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;174;-3776,-528;Inherit;False;156;BlockedMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;225;-3984,-400;Inherit;False;220;ShowStone;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;317;112,1520;Inherit;False;147;WorldUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;318;112,1600;Inherit;False;343;BuildingUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;348;112,1696;Inherit;False;196;GridSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;286;-1296,1264;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-5040,2656;Inherit;False;Property;_GapThickness;GapThickness;3;0;Create;True;0;0;0;False;0;False;0.1;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;173;-3536,-592;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;226;-3664,-400;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;346;416,1536;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;347;416,1632;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;249;-1120,1264;Inherit;False;Tree;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;349;544,1808;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-3648,-752;Inherit;False;Property;_CellAlpha;CellAlpha;4;0;Create;True;0;0;0;False;0;False;0.2;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;121;-4736,2656;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;-5040,2864;Inherit;False;Property;_LineThickness1;LineThickness;2;0;Create;True;0;0;0;False;0;False;0.1;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;219;-3408,-544;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;316;624,1616;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;322;848,1760;Inherit;False;249;Tree;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;323;848,1840;Inherit;False;208;Stone;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;366;656,1808;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-3264,-688;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;127;-4592,2864;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;198;-4640,2480;Inherit;False;196;GridSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;229;-1552,64;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;324;1072,1776;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;334;848,1600;Inherit;False;5;4;0;FLOAT;0;False;1;FLOAT;4;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;194;-4336,2832;Inherit;False;RoundedGrid;-1;;8;05da2913179bede4691ec7ba56515877;0;4;4;FLOAT2;8,8;False;3;FLOAT2;0,0;False;5;FLOAT;0.9;False;8;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;195;-4336,2976;Inherit;False;RoundedGrid;-1;;10;05da2913179bede4691ec7ba56515877;0;4;4;FLOAT2;8,8;False;3;FLOAT2;0,0;False;5;FLOAT;0.9;False;8;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;170;-3072,-688;Inherit;False;CellAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;197;-5072,1472;Inherit;False;196;GridSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;164;-1408,64;Inherit;False;VisionMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;80;208,1216;Inherit;False;Property;_MousePos;MousePos;13;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;321;1280,1632;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;171;-4192,2688;Inherit;False;170;CellAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;115;-4080,2928;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;193;-4208,2512;Inherit;False;RoundedGrid;-1;;12;05da2913179bede4691ec7ba56515877;0;4;4;FLOAT2;8,8;False;3;FLOAT2;0,0;False;5;FLOAT;0.9;False;8;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;143;-4816,1472;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;155;-4544,1072;Inherit;False;156;BlockedMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;165;-4544,1152;Inherit;False;164;VisionMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;608,1232;Inherit;False;Property;_FadeDistance;FadeDistance;6;0;Create;True;0;0;0;False;0;False;0;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;81;528,1120;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;327;1456,1632;Inherit;False;ResourceAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;350;1840,1504;Inherit;False;249;Tree;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;356;1808,1728;Inherit;False;208;Stone;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;125;-3904,2928;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;119;-3840,2544;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;132;-3936,3248;Inherit;False;Property;_GridAlpha;GridAlpha;5;0;Create;True;0;0;0;False;0;False;0.5;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;178;-3856,3104;Inherit;False;170;CellAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;138;-4656,1456;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;10,10;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;168;-4288,1104;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;83;944,1200;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;352;2048,1536;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;357;2016,1760;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-3600,2720;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;163;-3680,2992;Inherit;False;152;ObjPlacementMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;177;-3616,3104;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;182;-4448,1456;Inherit;False;TileUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;169;-4176,1104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;135;-4544,1264;Inherit;True;Property;_Texture0;Texture 0;15;0;Create;True;0;0;0;False;0;False;26c77a8cccd87cf4aa57a76272a7b384;26c77a8cccd87cf4aa57a76272a7b384;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;157;-4304,1008;Inherit;False;152;ObjPlacementMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;230;-4512,1792;Inherit;True;Property;_Texture1;Texture 0;18;0;Create;True;0;0;0;False;0;False;e7d6bdca26d788140a6a04c0d191ac17;26c77a8cccd87cf4aa57a76272a7b384;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;264;-4544,2208;Inherit;True;Property;_Texture2;Texture 0;19;0;Create;True;0;0;0;False;0;False;d32dcc8c18ad5204bbe46ea80d3f78a5;26c77a8cccd87cf4aa57a76272a7b384;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SaturateNode;85;1056,1200;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;353;2289.858,1565.07;Inherit;False;FinalTreeAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;358;2272,1792;Inherit;False;FinalStoneAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;130;-3472,2720;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;162;-3424,2960;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;158;-3984,1040;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;137;-4192,1328;Inherit;True;Property;_TextureSample1;Texture Sample 1;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;231;-4160,1744;Inherit;True;Property;_TextureSample4;Texture Sample 1;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;265;-4192,2160;Inherit;True;Property;_TextureSample5;Texture Sample 1;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.OneMinusNode;84;1232,1200;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;234;-4096,1632;Inherit;False;358;FinalStoneAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;266;-4128,2032;Inherit;False;353;FinalTreeAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;131;-3312,2720;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.7;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;-3712,1312;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;232;-3744,1696;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;267;-3776,2048;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;87;1424,1200;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;-3120,2720;Inherit;False;Grid;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;199;-3552,1312;Inherit;False;CrossTexture;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;233;-3584,1632;Inherit;False;StoneTexture;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;268;-3616,2048;Inherit;False;TreeTexture;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;312;1680,1200;Inherit;False;DistanceAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;201;-752,224;Inherit;False;199;CrossTexture;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;-720,304;Inherit;False;116;Grid;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;235;-752,384;Inherit;False;233;StoneTexture;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;269;-752,464;Inherit;False;268;TreeTexture;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;313;-800,576;Inherit;False;312;DistanceAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;329;-800,656;Inherit;False;327;ResourceAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;200;-432,336;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;328;-496,592;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-64,64;Inherit;False;Constant;_OffAlpha;OffAlpha;8;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;-240,384;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;107;-176,-704;Inherit;False;Constant;_NoVisionColor;NoVisionColor;9;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;106;304,-720;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;160;-1104,-1808;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;161;-1456,-2160;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;210;-1472,-1680;Inherit;False;156;BlockedMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;55;-1504,-1888;Inherit;False;Property;_InvalidGridColor;InvalidGridColor;7;0;Create;True;0;0;0;False;0;False;1,0,0,0;0.9215686,0.2313726,0.3529412,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;159;-1840,-2144;Inherit;False;Property;_CurrentPlacementGridColor;CurrentPlacementGridColor;10;0;Create;True;0;0;0;False;0;False;1,0,0,0;0.9921569,0.5882353,0.2666667,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;73;-1840,-2352;Inherit;False;Property;_DefaultGridColor;DefaultGridColor;11;0;Create;True;0;0;0;False;0;False;0.1490196,0.8705882,0.5058824,1;0.8196079,0.8470588,0.8784314,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;154;-1824,-1920;Inherit;False;152;ObjPlacementMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;211;528,-720;Inherit;False;GridColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;89;112,80;Inherit;False;Property;_ShowGrid;ShowGrid;14;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;228;112,-192;Inherit;False;211;GridColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;227;-160,-496;Inherit;False;164;VisionMap;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;331;96,-480;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;332;-320,-400;Inherit;False;327;ResourceAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;333;-112,-400;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;214;-160,-912;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;213;-544,-656;Inherit;False;353;FinalTreeAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;256;-608,-1184;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;261;-896,-1328;Inherit;False;254;ValidColour;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;257;-928,-992;Inherit;False;358;FinalStoneAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;361;80,-2064;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;254;-848,-1744;Inherit;False;ValidColour;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;363;256,-2064;Inherit;False;ResourceColour;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;364;-928,-1168;Inherit;False;363;ResourceColour;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;365;-544,-864;Inherit;False;363;ResourceColour;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;359;-1504,-48;Inherit;False;ResourceAvailable;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;360;-288,-2224;Inherit;False;Property;_UnavailableResourceColor;UnavailableResourceColor;9;0;Create;True;0;0;0;False;0;False;0.9215686,0.2313726,0.3529412,1;0.1254902,0.7490196,0.4196078,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;258;-288,-2032;Inherit;False;Property;_AvailableResourceColor;AvailableResourceColor;8;0;Create;True;0;0;0;False;0;False;0.1254902,0.7490196,0.4196078,1;0.1254902,0.7490196,0.4196078,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;362;-256,-1824;Inherit;False;359;ResourceAvailable;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;26;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;27;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;True;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;28;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;29;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;30;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;31;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;32;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormals;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;33;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;24;272,-768;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;25;432,-16;Float;False;True;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;GridTerrain;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;7;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;;0;0;Standard;22;Surface;1;638601996763276826;  Blend;0;0;Two Sided;1;0;Forward Only;0;638601996277680676;Cast Shadows;0;638601996248904480;  Use Shadow Threshold;0;0;Receive Shadows;0;638617705477497027;GPU Instancing;0;0;LOD CrossFade;0;0;Built-in Fog;0;638601996230821323;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;False;True;False;False;True;True;True;False;False;;False;0
WireConnection;47;0;46;1
WireConnection;47;1;46;3
WireConnection;202;0;47;0
WireConnection;103;0;203;0
WireConnection;103;1;104;0
WireConnection;105;0;103;0
WireConnection;92;0;105;0
WireConnection;92;1;100;0
WireConnection;93;0;92;0
WireConnection;93;1;94;0
WireConnection;147;0;93;0
WireConnection;309;0;308;0
WireConnection;145;0;149;0
WireConnection;145;1;148;0
WireConnection;18;0;17;0
WireConnection;18;1;204;0
WireConnection;152;0;145;1
WireConnection;336;0;342;0
WireConnection;336;1;104;0
WireConnection;304;0;18;2
WireConnection;338;0;336;0
WireConnection;220;0;262;0
WireConnection;307;0;305;0
WireConnection;340;0;338;0
WireConnection;340;1;100;0
WireConnection;245;0;243;0
WireConnection;341;0;340;0
WireConnection;341;1;94;0
WireConnection;253;0;263;0
WireConnection;244;0;307;0
WireConnection;244;1;245;0
WireConnection;244;2;355;0
WireConnection;311;0;310;0
WireConnection;156;0;18;1
WireConnection;196;0;122;0
WireConnection;306;0;305;0
WireConnection;343;0;341;0
WireConnection;285;0;284;0
WireConnection;208;0;244;0
WireConnection;286;0;306;0
WireConnection;286;1;285;0
WireConnection;286;2;354;0
WireConnection;173;0;175;0
WireConnection;173;1;174;0
WireConnection;226;0;225;0
WireConnection;226;1;216;0
WireConnection;346;0;317;0
WireConnection;346;1;348;0
WireConnection;347;0;318;0
WireConnection;347;1;348;0
WireConnection;249;0;286;0
WireConnection;349;0;320;0
WireConnection;121;0;128;0
WireConnection;219;0;173;0
WireConnection;219;1;226;0
WireConnection;316;0;346;0
WireConnection;316;1;347;0
WireConnection;366;0;349;0
WireConnection;176;0;118;0
WireConnection;176;1;219;0
WireConnection;127;0;121;0
WireConnection;127;1;120;0
WireConnection;229;0;18;4
WireConnection;324;0;322;0
WireConnection;324;1;323;0
WireConnection;334;0;316;0
WireConnection;334;1;366;0
WireConnection;194;4;198;0
WireConnection;194;5;121;0
WireConnection;195;4;198;0
WireConnection;195;5;127;0
WireConnection;170;0;176;0
WireConnection;164;0;229;0
WireConnection;321;0;334;0
WireConnection;321;1;324;0
WireConnection;115;0;194;0
WireConnection;115;1;195;0
WireConnection;193;4;198;0
WireConnection;193;5;121;0
WireConnection;143;0;197;0
WireConnection;143;1;197;0
WireConnection;81;0;202;0
WireConnection;81;1;80;0
WireConnection;327;0;321;0
WireConnection;125;0;115;0
WireConnection;119;0;193;0
WireConnection;119;2;171;0
WireConnection;138;0;143;0
WireConnection;168;0;155;0
WireConnection;168;1;165;0
WireConnection;83;0;81;0
WireConnection;83;1;82;0
WireConnection;352;0;350;0
WireConnection;352;1;327;0
WireConnection;357;0;356;0
WireConnection;357;1;327;0
WireConnection;126;0;119;0
WireConnection;126;1;125;0
WireConnection;177;0;178;0
WireConnection;177;1;132;0
WireConnection;182;0;138;0
WireConnection;169;0;168;0
WireConnection;85;0;83;0
WireConnection;353;0;352;0
WireConnection;358;0;357;0
WireConnection;130;0;126;0
WireConnection;162;0;163;0
WireConnection;162;1;177;0
WireConnection;158;0;157;0
WireConnection;158;1;169;0
WireConnection;137;0;135;0
WireConnection;137;1;182;0
WireConnection;231;0;230;0
WireConnection;231;1;182;0
WireConnection;265;0;264;0
WireConnection;265;1;182;0
WireConnection;84;0;85;0
WireConnection;131;0;130;0
WireConnection;131;4;162;0
WireConnection;146;0;158;0
WireConnection;146;1;137;5
WireConnection;232;0;234;0
WireConnection;232;1;231;5
WireConnection;267;0;266;0
WireConnection;267;1;265;5
WireConnection;87;0;84;0
WireConnection;116;0;131;0
WireConnection;199;0;146;0
WireConnection;233;0;232;0
WireConnection;268;0;267;0
WireConnection;312;0;87;0
WireConnection;200;0;201;0
WireConnection;200;1;117;0
WireConnection;200;2;235;0
WireConnection;200;3;269;0
WireConnection;328;0;313;0
WireConnection;328;1;329;0
WireConnection;88;0;200;0
WireConnection;88;1;328;0
WireConnection;106;0;214;0
WireConnection;106;1;107;0
WireConnection;106;2;331;0
WireConnection;160;0;161;0
WireConnection;160;1;55;0
WireConnection;160;2;210;0
WireConnection;161;0;73;0
WireConnection;161;1;159;0
WireConnection;161;2;154;0
WireConnection;211;0;106;0
WireConnection;89;1;90;0
WireConnection;89;0;88;0
WireConnection;331;0;227;0
WireConnection;331;1;333;0
WireConnection;333;0;332;0
WireConnection;214;0;256;0
WireConnection;214;1;365;0
WireConnection;214;2;213;0
WireConnection;256;0;261;0
WireConnection;256;1;364;0
WireConnection;256;2;257;0
WireConnection;361;0;360;5
WireConnection;361;1;258;5
WireConnection;361;2;362;0
WireConnection;254;0;160;0
WireConnection;363;0;361;0
WireConnection;359;0;18;3
WireConnection;25;2;228;0
WireConnection;25;3;89;0
ASEEND*/
//CHKSM=9DD8DA7564320D94CB2C534777CF9CF2B9E2158E