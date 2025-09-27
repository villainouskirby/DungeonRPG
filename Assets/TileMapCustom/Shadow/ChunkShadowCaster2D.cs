using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(ShadowCaster2D))]
public class ChunkShadowCaster2D : MonoBehaviour
{
    private ShadowCaster2D _caster;

    private MethodInfo _updateSphere;
    private FieldInfo _meshField;
    private FieldInfo _lbField;

    void Awake()
    {
        _caster = GetComponent<ShadowCaster2D>();
        LoadFieldAndMethod();
        _caster.useRendererSilhouette = false;
        _caster.castsShadows = true;
        //_caster.selfShadows = true;
    }

    private void LoadFieldAndMethod()
    {
        _updateSphere = typeof(ShadowCaster2D)
            .GetMethod("UpdateBoundingSphere", BindingFlags.Instance | BindingFlags.NonPublic);
        _meshField = typeof(ShadowCaster2D)
            .GetField("m_Mesh", BindingFlags.Instance | BindingFlags.NonPublic);
        _lbField = typeof(ShadowCaster2D)
            .GetField("m_LocalBounds", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public void ApplyChunkShadow(Mesh shadowMesh)
    {
        _meshField.SetValue(_caster, shadowMesh);

        _lbField.SetValue(_caster, shadowMesh.bounds);
        _updateSphere.Invoke(_caster, null);
    }
}
