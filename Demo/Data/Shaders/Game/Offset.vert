#version 420

uniform vec2 offset = vec2(0.0);

in vec3 vertex_position;
in vec2 uv;

out vec2 fragUV;

void main() 
{
    gl_Position = vec4(vertex_position, 1.0);
    fragUV = uv + offset;
}
