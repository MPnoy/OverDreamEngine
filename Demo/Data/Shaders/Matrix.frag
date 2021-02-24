#version 420

layout(binding = 0) uniform sampler2D textureSampler;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 o = texture(textureSampler, fragUV);
    o.rgb *= o.a;
    out_Color = o;
}
