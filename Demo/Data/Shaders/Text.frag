#version 420

uniform sampler2D tex;

in vec2 fragUV;
in vec4 fragColor;
out vec4 out_Color;

void main()
{
    vec4 OutVec = vec4(fragColor.rgb, fragColor.a * texture(tex, fragUV).r);
    OutVec.a = 1 - OutVec.a;
    OutVec.a *= OutVec.a;
    OutVec.a = 1 - OutVec.a;
    out_Color = OutVec;
}
