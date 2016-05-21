using System.Collections.Generic;
using System.Diagnostics;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

class Renderer : SceneVisitor
{

    //Texture parameters
    private IShaderParam TextureParam;
    private IShaderParam TexMixParam;

    public Dictionary<string, ITexture> textureDict; 
    public Dictionary<string, ShaderEffect> shaderEffectDictr;
    private ShaderEffect _shaderEffect;

    public RenderContext RC;
    public IShaderParam AlbedoParam;
    public IShaderParam ShininessParam;
    public IShaderParam SpecFactorParam;
    public IShaderParam SpecColorParam;
    public IShaderParam AmbientColorParam;
    public float4x4 View;
    private Dictionary<MeshComponent, Mesh> _meshes = new Dictionary<MeshComponent, Mesh>();
    private CollapsingStateStack<float4x4> _model = new CollapsingStateStack<float4x4>();

    private Mesh LookupMesh(MeshComponent mc)
    {
        Mesh mesh;
        if (!_meshes.TryGetValue(mc, out mesh))
        {
            mesh = new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                UVs = mc.UVs,
                Triangles = mc.Triangles,
            };
        }
        return mesh;
    }

    public Renderer(RenderContext rc)
    {
        RC = rc;
        // Initialize the shader(s)
        var vertsh = AssetStorage.Get<string>("VertexShader.vert");
        var pixsh = AssetStorage.Get<string>("PixelShader.frag");
        var pixShEff = AssetStorage.Get<string>("PixelShaderEffect.frag");
        var vertShaderEff = AssetStorage.Get<string>("VertexShaderEffect.frag");
        var shader = RC.CreateShader(vertsh, pixsh);

        RC.SetShader(shader);

        AlbedoParam = RC.GetShaderParam(shader, "albedo");
        ShininessParam = RC.GetShaderParam(shader, "shininess");
        SpecFactorParam = RC.GetShaderParam(shader, "specfactor");
        SpecColorParam = RC.GetShaderParam(shader, "speccolor");
        AmbientColorParam = RC.GetShaderParam(shader, "ambientcolor");

        //Texture init
        ImageData leaves = AssetStorage.Get<ImageData>("Leaves.jpg");
        ImageData leaves_2 = AssetStorage.Get<ImageData>("Leaves_2.jpg");
        ImageData sphere = AssetStorage.Get<ImageData>("litsphere.jpg");

        textureDict = new Dictionary<string, ITexture>();
        textureDict.Add("leaves", RC.CreateTexture(leaves));
        textureDict.Add("leaves2", RC.CreateTexture(leaves_2));
        textureDict.Add("sphere", RC.CreateTexture(sphere));

        _shaderEffect = new ShaderEffect(
            new []
            {
                new EffectPassDeclaration()
                {
                    VS = vertsh,
                    PS = pixsh,
                    StateSet = new RenderStateSet
                    {
                        ZEnable = true,
                        CullMode = Cull.Counterclockwise
                    }
                }        
            },
            new[]
            {
                new EffectParameterDeclaration {Name = "albedo", Value = float3.One},
                new EffectParameterDeclaration {Name = "shininess", Value = 1.0f},
                new EffectParameterDeclaration {Name = "specfactor", Value = 1.0f},
                new EffectParameterDeclaration {Name = "speccolor", Value = float3.Zero},
                new EffectParameterDeclaration {Name = "ambientcolor", Value = float3.Zero},
                new EffectParameterDeclaration {Name = "texture", Value = textureDict["leaves"]},
                new EffectParameterDeclaration {Name = "texmix", Value = 0.0f}
        });

        ShaderEffect shaderEffectToon = new ShaderEffect(
                new[]
                {
                    new EffectPassDeclaration
                    {
                        VS = vertShaderEff,
                        PS = pixShEff,
                        StateSet = new RenderStateSet
                        {
                            // Fix from E-Mail
                            ZEnable = true,
                            CullMode = Cull.Clockwise
                        }
                    },
                    //new EffectPassDeclaration
                    //{
                    //    VS = cellVs,
                    //    PS = cellPs,
                    //    StateSet = new RenderStateSet
                    //    {
                    //        ZEnable = true,
                    //        CullMode = Cull.Counterclockwise
                    //    }
                    //}
                },
                new[]
                {
                    new EffectParameterDeclaration {Name = "albedo", Value = float3.One},
                    new EffectParameterDeclaration {Name = "shininess", Value = 1.0f},
                    new EffectParameterDeclaration {Name = "specfactor", Value = 1.0f},
                    new EffectParameterDeclaration {Name = "speccolor", Value = float3.Zero},
                    new EffectParameterDeclaration {Name = "ambientcolor", Value = float3.Zero},
                    new EffectParameterDeclaration {Name = "texture", Value = textureDict["sphere"]},
                    new EffectParameterDeclaration {Name = "texmix", Value = 1.0f},
                    new EffectParameterDeclaration {Name = "linecolor", Value = float4.Zero},
                    new EffectParameterDeclaration {Name = "linewidth", Value = float2.One * 1.5f}

                });

        shaderEffectDictr = new Dictionary<string, ShaderEffect>();
        shaderEffectDictr.Add("Tree.1", shaderEffectToon);
        shaderEffectDictr.Add("Tree.3", shaderEffectToon);
        shaderEffectDictr.Add("Tree.2", shaderEffectToon);

        TextureParam = RC.GetShaderParam(shader, "texture");
        TexMixParam = RC.GetShaderParam(shader, "texmix");

        _shaderEffect.AttachToContext(RC);
        shaderEffectToon.AttachToContext(RC);
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
        //RC.Render(LookupMesh(mesh));
        if (shaderEffectDictr.ContainsKey(CurrentNode.Name))
        {
            _shaderEffect.RenderMesh(LookupMesh(mesh));
        }
    }

    [VisitMethod]
    void OnMaterial(MaterialComponent material)
    {
        if (material.HasDiffuse)
        {
            RC.SetShaderParam(AlbedoParam, material.Diffuse.Color);
        }
        else
        {
            RC.SetShaderParam(AlbedoParam, float3.Zero);
        }

        if (material.HasSpecular)
        {
            RC.SetShaderParam(ShininessParam, material.Specular.Shininess);
            RC.SetShaderParam(SpecFactorParam, material.Specular.Intensity);
            RC.SetShaderParam(SpecColorParam, material.Specular.Color);
        }
        else
        {
            RC.SetShaderParam(ShininessParam, 0);
            RC.SetShaderParam(SpecFactorParam, 0);
            RC.SetShaderParam(SpecColorParam, float3.Zero);
        }

        if (material.HasEmissive)
        {
            RC.SetShaderParam(AmbientColorParam, material.Emissive.Color);
        }
        else
        {
            RC.SetShaderParam(AmbientColorParam, float3.Zero);
        }

        if (material.Diffuse.Texture == "Leaves.jpg")
        {
            RC.SetShaderParamTexture(TextureParam, textureDict["leaves"]);
            RC.SetShaderParam(TexMixParam, 1.0f);
        }
        else
        {
            RC.SetShaderParam(TexMixParam, 0.0f);
        }

        //if (shaderEffectDictr.ContainsKey(CurrentNode.Name))
        //{
        //    RC.SetShaderParam(shaderEffectDictr[CurrentNode.Name]);
        //}
    }
    [VisitMethod]
    void OnTransform(TransformComponent xform)
    {
        _model.Tos *= xform.Matrix();
        RC.ModelView = View * _model.Tos;
    }
}

