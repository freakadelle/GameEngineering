using System;
using System.Collections.Generic;
using System.Diagnostics;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

class Renderer : SceneVisitor
{
    public RenderContext RC;

    public IShaderParam AlbedoParam;
    public IShaderParam ShininessParam;
    public IShaderParam SpecularIntParam;
    public IShaderParam SpecularColParam;
    public IShaderParam LightPosParam;
    private float shininess;

    public float4x4 View;
    private Dictionary<MeshComponent, Mesh> _meshes = new Dictionary<MeshComponent, Mesh>();
    private CollapsingStateStack<float4x4> _model = new CollapsingStateStack<float4x4>();

    public Renderer(RenderContext rc)
    {
        RC = rc;

        // Initialize the shader(s)
        var vertsh = AssetStorage.Get<string>("VertexShader.vert");
        var pixsh = AssetStorage.Get<string>("PixelShader.frag");
        var shader = RC.CreateShader(vertsh, pixsh);
        RC.SetShader(shader);

        shininess = 50.0f;

        AlbedoParam = RC.GetShaderParam(shader, "albedo");
        ShininessParam = RC.GetShaderParam(shader, "shininess");
        SpecularIntParam = RC.GetShaderParam(shader, "specInt");
        SpecularColParam = RC.GetShaderParam(shader, "specCol");
        LightPosParam = RC.GetShaderParam(shader, "lightdir");
    }

    private Mesh LookupMesh(MeshComponent mc)
    {
        Mesh mesh;
        if (!_meshes.TryGetValue(mc, out mesh))
        {
            mesh = new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                Triangles = mc.Triangles
            };
            _meshes[mc] = mesh;
        }
        return mesh;
    }

    protected override void InitState()
    {
        _model.Clear();
        _model.Tos = float4x4.Identity;
    }

    protected override void PushState()
    {
        _model.Push();
    }

    protected override void PopState()
    {
        _model.Pop();
        RC.ModelView = View * _model.Tos;
    }

    [VisitMethod]
    void OnMesh(MeshComponent mesh)
    {
        RC.Render(LookupMesh(mesh));
    }

    [VisitMethod]
    private void OnMaterial(MaterialComponent material)
    {
        RC.SetShaderParam(AlbedoParam, material.Diffuse.Color);
        RC.SetShaderParam(ShininessParam, shininess);
        RC.SetShaderParam(SpecularColParam, material.Specular.Color);
        RC.SetShaderParam(SpecularIntParam, material.Specular.Intensity);
    }

    [VisitMethod]
    void OnTransform(TransformComponent xform)
    {
        _model.Tos *= xform.Matrix();
        RC.ModelView = View * _model.Tos;
    }

    //PROPERTIES

    public float Shininess
    {
        get { return shininess; }
        set
        {
            shininess = value;
            shininess = Math.Max(shininess, 1.0f);
            shininess = Math.Min(shininess, 500.0f);
        }
    }

    public void newLightPos(float3 pos)
    {
        RC.SetShaderParam(LightPosParam, pos);
    }
}