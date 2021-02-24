#version 420

uniform vec4 WriteRect;
uniform vec2 WriteMultiplier;

in vec3 vertex_position;
in vec2 uv;

out vec2 fragUV;

void main()
{
    gl_Position = vec4(mix((WriteRect.xy - vec2(0.5)) * 2.0, (WriteRect.zw - vec2(0.5)) * 2.0, (vertex_position.xy + vec2(1.0)) / 2.0), vertex_position.z, 1);
    fragUV = (uv - 0.5) * WriteMultiplier + 0.5;
}
