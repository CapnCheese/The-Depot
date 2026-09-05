#ifndef GRASS_INSTANCE_INCLUDE
#define GRASS_INSTANCE_INCLUDE

StructuredBuffer<float4x4> _TransformBuffer;

void GetInstanceWorldPosition_float(float3 vertexOS, float instanceID_float, out float3 worldPos, out float3 rootWorldPos)
{
    uint instanceID = (uint)instanceID_float;
    
    float4x4 mat = _TransformBuffer[instanceID];
    
    worldPos = mul(mat, float4(vertexOS, 1.0)).xyz;
    
    rootWorldPos = float3(mat[0][3], mat[1][3], mat[2][3]);
}
#endif