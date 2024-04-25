@vertex
@include(includes/camera.inc)
uniform mat4x4 World;

in vec3 vertex_position;
in vec2 vertex_texture1;
in float vertex_color;

out vec2 tex;

void main()
{
    vec4 pos = (ViewProjection * World) * vec4(vertex_position, 1.0);
    gl_Position = pos;
    tex = vertex_texture1;
}

@fragment
uniform sampler2D Texture;
in vec2 tex;
out vec4 out_color;

void main()
{
    vec4 sampled = texture(Texture, tex);
    if(sampled.a < 0.5)
        discard;
    out_color = sampled;
}
