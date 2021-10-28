#include "Tessellation.cginc"
struct appdata_tess {
    float4 vertex : INTERNALTESSPOS;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    float4 texcoord2 : TEXCOORD2;
    float4 texcoord3 : TEXCOORD3;
    fixed4 color : COLOR;
};

struct OutputPatchConstant {
    float edge[3]         : SV_TessFactor;
    float inside          : SV_InsideTessFactor;
    float3 vTangent[4]    : TANGENT;
    float2 vUV[4]         : TEXCOORD;
    float3 vTanUCorner[4] : TANUCORNER;
    float3 vTanVCorner[4] : TANVCORNER;
    float4 vCWts          : TANWEIGHTS;
};
appdata_tess tessvert (appdata_full v) {
    appdata_tess o;
    o.vertex = v.vertex;
    o.tangent = v.tangent;
    o.normal = v.normal;
    o.texcoord = v.texcoord;
    o.texcoord1 = v.texcoord1;
    o.texcoord2 = v.texcoord2;
    o.texcoord3 = v.texcoord3;
    o.color = v.color;
    return o;
}
float3 ProjectPositionToTangentSpace( float3 position, float3 plane_position, float3 plane_normal)
{
    float projectValue = dot( ( position - plane_position ), plane_normal );
    return ( position - projectValue * plane_normal * _TessellationPhongStretch);
}
float4 Tessellation(appdata_tess v, appdata_tess v1, appdata_tess v2){
    float4 tess = UnityDistanceBasedTess(v.vertex, v1.vertex, v2.vertex, _TessellationEndDistance, _TessellationBeginDistance, _TessellationMaxDensity);
    float  densityMask = clamp(tex2Dlod (_TessellationDensityMask, float4( TRANSFORM_TEX(v.texcoord, _TessellationDensityMask), 0, 0)), 0.01, 1.0);
    return tess * densityMask;
}
OutputPatchConstant hullconst (InputPatch<appdata_tess,3> v) {
    OutputPatchConstant o = (OutputPatchConstant)0;
    float4 ts = Tessellation( v[0], v[1], v[2] );
    o.edge[0] = ts.x;
    o.edge[1] = ts.y;
    o.edge[2] = ts.z;
    o.inside = ts.w;
    return o;
}
[domain("tri")]
[partitioning("fractional_odd")]
[outputtopology("triangle_cw")]
[patchconstantfunc("hullconst")]
[outputcontrolpoints(3)]
appdata_tess hull (InputPatch<appdata_tess,3> v, uint id : SV_OutputControlPointID) {
    return v[id];
}
[domain("tri")]
VertexOutput domain (OutputPatchConstant tessFactors, const OutputPatch<appdata_tess,3> vi, float3 bary : SV_DomainLocation) {
    appdata_full v = (appdata_full)0;
    v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
    v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
    v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
    v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
    v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
    v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
    v.texcoord3 = vi[0].texcoord3*bary.x + vi[1].texcoord3*bary.y + vi[2].texcoord3*bary.z;

    float3 phongPos = float3( 0.0f, 0.0f, 0.0f );
    phongPos += ( bary.x * ProjectPositionToTangentSpace( v.vertex.xyz, vi[ 0 ].vertex, normalize(vi[ 0 ].normal) ) );
    phongPos += ( bary.y * ProjectPositionToTangentSpace( v.vertex.xyz, vi[ 1 ].vertex, normalize(vi[ 1 ].normal) ) );
    phongPos += ( bary.z * ProjectPositionToTangentSpace( v.vertex.xyz, vi[ 2 ].vertex, normalize(vi[ 2 ].normal) ) );

    v.vertex = float4(phongPos, v.vertex.w);

    VertexOutput o = vert(v);
    return o;
}