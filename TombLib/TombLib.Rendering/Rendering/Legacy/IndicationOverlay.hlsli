void ApplyBrushOverlay(inout float3 rgb, inout float alpha, bool updateAlpha, float4 svPos, float3 worldPos, float lineWidthFactor)
{
    if (BrushShape == 0)
        return;

    float2 delta = worldPos.xz - BrushCenter.xz;
    float dist;
    if (BrushShape == 1)
        dist = length(delta); // Circle
    else
        dist = max(abs(delta.x), abs(delta.y)); // Square

    float edge = abs(dist - BrushCenter.w);
    float lineWidth = (lineWidthFactor * 2048.0f) / svPos.w;
    float fw = max(fwidth(dist), 0.001f);

    // Negate-style fill (invert background color inside brush)

    float fillAlpha = step(dist, BrushCenter.w) * BrushColor.w;

    float3 excludeColor = rgb + BrushColor.xyz - 2.0f * rgb * BrushColor.xyz;
    float3 diffColor    = abs(rgb - BrushColor.xyz);

    // Combine both blend modes
    float3 combined = saturate(excludeColor + diffColor);

    rgb = lerp(rgb, combined, fillAlpha);

    if (updateAlpha)
        alpha = max(alpha, fillAlpha);

    // White contour ring at the brush edge.

    float outerEdge = edge / fw;
    float contourAlpha = saturate(1.0f - outerEdge / max(lineWidth, 0.001f));
    rgb = lerp(rgb, float3(1, 1, 1), contourAlpha);

    if (updateAlpha)
        alpha = max(alpha, contourAlpha);

    // Rotation indicator line extending from the brush center.

    if (BrushRotation >= 0.0f)
    {
        float rotRad = BrushRotation * 3.14159265f / 180.0f;
        float2 rotDir = float2(sin(rotRad), cos(rotRad));

        float along = dot(delta, rotDir);
        float perp = abs(dot(delta, float2(-rotDir.y, rotDir.x)));

        float perpFw = max(fwidth(perp), 0.001f);
        float perpNorm = perp / perpFw;

        float lineExtent;

        if (BrushShape == 1) // circle
            lineExtent = BrushCenter.w;
        else // square
            lineExtent = BrushCenter.w / max(abs(rotDir.x), abs(rotDir.y));

        lineExtent = max(lineExtent, 1024.0f);

        float withinLine = step(0.0f, along) * step(along, lineExtent);

        float lineAlpha = saturate(1.0f - perpNorm / max(lineWidth * 0.7f, 0.001f)) * withinLine;
        rgb = lerp(rgb, float3(1, 1, 1), lineAlpha);

        if (updateAlpha)
            alpha = max(alpha, lineAlpha);
    }
}

void ApplyDofOverlay(inout float3 rgb, inout float alpha, bool updateAlpha, float3 worldPos)
{
    int dofMode = (int)round(DofColorStrength.w);

    if (dofMode == 0)
        return;

    float3 viewDirection = DofDirectionDistance.xyz;
    float directionLength = length(viewDirection);

    if (directionLength <= 0.0001f)
        return;

    viewDirection /= directionLength;

    float3 focusPoint = DofCenterRange.xyz + viewDirection * DofDirectionDistance.w;
    float signedDepth = dot(worldPos - focusPoint, viewDirection);

    if (dofMode == 2)
    {
        if (signedDepth >= 0.0f)
            return;

        signedDepth = -signedDepth;
    }
    else if (dofMode == 3)
    {
        if (signedDepth <= 0.0f)
            return;
    }
    else
    {
        signedDepth = abs(signedDepth);
    }

    float focusRange = max(DofCenterRange.w, 1.0f);
    float gradient = saturate(signedDepth / focusRange);

    if (gradient <= 0.0f)
        return;

    float3 darkening = lerp(float3(1.0f, 1.0f, 1.0f), DofColorStrength.xyz, gradient);
    rgb *= darkening;
}
