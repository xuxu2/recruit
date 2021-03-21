using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Sample;

public class MyFrameWork : MonoBehaviour
{
    private T LoadFromJson<T>(string path, string file)
    {
        using (FileStream fileStream = new FileStream(StrBuilderEx.Concat(path, "/", file), FileMode.Open))
        {
            byte[] _data = new byte[fileStream.Length];
            fileStream.Read(_data, 0, _data.Length);

            return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(_data));
        }
    }

    readonly
    Vector2[] _base_UVs = {
        new Vector2(.75f,.5f),
        new Vector2(.5f,.5f),
        new Vector2(.5f,1),

        new Vector2(.5f,1),
        new Vector2(.75f,1),
        new Vector2(.75f,.5f)};

    readonly
    Vector2[] _updown_UVs = {
        new Vector2(1,0),
        new Vector2(.75f,0),
        new Vector2(.75f,1),

        new Vector2(.75f,1),
        new Vector2(1,1),
        new Vector2(1,0)};

    readonly
    Vector2[] _front_UVs = {
        new Vector2(.5f,0),
        new Vector2(0,0),
        new Vector2(0,.5f),

        new Vector2(0,.5f),
        new Vector2(.5f,.5f),
        new Vector2(.5f,0)};

    public Material material;
    private List<Mesh> meshes = new List<Mesh>();
    private List<MaterialPropertyBlock> propBlocks = new List<MaterialPropertyBlock>();

    public static float GetAngle(Vector2 pos1, Vector2 pos2)
    {
        Vector2 dir = (pos2 - pos1).normalized;
        float angle = Mathf.Atan2(dir.x, dir.y);

        return angle;
    }

    private Vector2 GetUVs(Vector3 norm, int idx)
    {
        if (norm == Vector3.up || norm == Vector3.down)
        {
            return _updown_UVs[idx];
        }
        else
        {
            var angle = GetAngle(new Vector2(norm.x, norm.z), Vector2.up);

            if (0 <= angle && angle <= 0.32f)
            {
                return _front_UVs[idx];
            }

            return _base_UVs[idx];
        }
    }

    void Start()
    {
        var _data = LoadFromJson<ApiResponse>(Application.dataPath + "/Samples/json", "dong.json");

        if (_data.data.Count > 0)
        {
            material = new Material((Material)Resources.Load("Sample", typeof(Material)));

            float _maxHeight = 0.0f;
            _data.data.ForEach(o =>
            {
                Mesh _mesh = new Mesh();

                List<Vector3> _vertices = new List<Vector3>();
                List<int> _triangles = new List<int>();

                for (int i = 0; i < 2; ++i)
                {
                    byte[] _bytes = Convert.FromBase64String(o.roomtypes[0].coordinatesBase64s[i]);
                    float[] _floats = new float[_bytes.Length / 4];
                    Debug.Assert(_floats.Length % 3 == 0);
                    Buffer.BlockCopy(_bytes, 0, _floats, 0, _bytes.Length);

                    for (int j = 0; j < _floats.Length;)
                    {
                        //Vector3 _vert = new Vector3(_floats[j++], _floats[j++], _floats[j++]);
                        Vector3 _vert = new Vector3();
                        _vert.x = _floats[j++];
                        _vert.z = _floats[j++];
                        _vert.y = _floats[j++];

                        if (_maxHeight < _vert.y) _maxHeight = _vert.y;

                        _vertices.Add(_vert);
                        _triangles.Add(_triangles.Count);
                    }
                }

                _mesh.SetVertices(_vertices);
                _mesh.SetTriangles(_triangles, 0, false);
                _mesh.RecalculateNormals();

                //-- UV
                List<Vector2> _UVs = new List<Vector2>();
                Vector3 _prevNorm = new Vector3();
                int _normIdx = 0;
                for (int i = 0; i < _mesh.normals.Length; ++i)
                {
                    if (_mesh.normals[i] != _prevNorm || _normIdx > 5)
                    {
                        _normIdx = 0;
                        _prevNorm = _mesh.normals[i];
                    }

                    var _uv = GetUVs(_mesh.normals[i], _normIdx++);
                    _UVs.Add(_uv);
                }

                _mesh.SetUVs(0, _UVs);
                _mesh.OptimizeReorderVertexBuffer();

                meshes.Add(_mesh);

                var _floor = Mathf.Floor(_maxHeight / 3);

                var propBlock = new MaterialPropertyBlock();
                int propertyID = Shader.PropertyToID("_BaseMap_ST");

                propBlock.SetVector(propertyID, new Vector2(1f, _floor));
                propBlocks.Add(propBlock);
                _maxHeight = 0.0f;
            });
        }

    }
    private void OnRender()
    {
        //Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
        //meshes.ForEach(o => { Graphics.DrawMesh(o, Vector3.zero, Quaternion.identity, material, 0, null, 0, propBlock); });

        for (int i = 0; i < meshes.Count; ++i)
        {
            Graphics.DrawMesh(meshes[i], Vector3.zero, Quaternion.identity, material, 0, null, 0, propBlocks[i]);
        }
    }
    void Update()
    {
        OnRender();
    }
}
