#version 420

uniform sampler2D blitInput;

uniform vec4 _MaskStart;
uniform vec4 _MaskRect;
uniform vec4 _Color = vec4(1.0);

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 OutVec = texture(blitInput, fragUV);
    bool MaskStartFlag = (fragUV.y < _MaskStart.z) || ((fragUV.y < _MaskStart.y) && (fragUV.x > _MaskStart.x));
    if (MaskStartFlag && ((fragUV.y < _MaskRect.w) || (fragUV.y < _MaskRect.y && fragUV.x > _MaskRect.z)))
    {
        out_Color = vec4(0.0);
        return;
    }
    if (MaskStartFlag &&
        (fragUV.y < _MaskRect.y && fragUV.y > _MaskRect.w) &&
        (fragUV.x > _MaskRect.x && fragUV.x < _MaskRect.z))
    {
        OutVec.a *= 1.0 - (fragUV.x - _MaskRect.x) / (_MaskRect.z - _MaskRect.x);
    }
    out_Color = OutVec * _Color;
}