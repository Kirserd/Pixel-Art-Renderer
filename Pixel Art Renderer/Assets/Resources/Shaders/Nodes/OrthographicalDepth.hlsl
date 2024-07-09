void OrthographicalDepth_float(in float rawDepth, in float persp, out float orthDepth)
{
    float ortho = (_ProjectionParams.z - _ProjectionParams.y) * (1 - rawDepth) + _ProjectionParams.y;
    orthDepth = lerp(persp, ortho, unity_OrthoParams.w) / _ProjectionParams.z;
}