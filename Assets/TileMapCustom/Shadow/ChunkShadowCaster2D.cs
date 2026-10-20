using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(ShadowCaster2D))]
public class ChunkShadowCaster2D : MonoBehaviour
{
    // URP exposes shape generation publicly, but not the caster's shape source.
    // Keep the two version-specific bindings here, rather than modifying package code.
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly FieldInfo ShadowMeshField = typeof(ShadowCaster2D).GetField("m_ShadowMesh", PrivateInstance);
    private static readonly FieldInfo CastingSourceField = typeof(ShadowCaster2D).GetField("m_ShadowCastingSource", PrivateInstance);
    private ShadowCaster2D _caster;

    public void ApplyChunkShadow(Mesh shadowMesh)
    {
        if (shadowMesh == null)
            throw new ArgumentNullException(nameof(shadowMesh));

        // The pool can call this while inactive, before this component's Awake.
        if (_caster == null)
            _caster = GetComponent<ShadowCaster2D>();
        if (ShadowMeshField == null || CastingSourceField == null)
            throw new NotSupportedException("The installed URP shadow caster layout is not supported.");

        var shape = ShadowMeshField.GetValue(_caster) as ShadowShape2D;
        if (shape == null)
            throw new InvalidOperationException("ShadowCaster2D has not initialized its shadow shape.");

        GetOutline(shadowMesh, out Vector3[] points, out int[] edges);
        using var vertices = new NativeArray<Vector3>(points, Allocator.Temp);
        using var indices = new NativeArray<int>(edges, Allocator.Temp);
        // A provider source without a component keeps URP from replacing the supplied
        // geometry with the prefab's placeholder shape on its next Update.
        CastingSourceField.SetValue(_caster, Enum.Parse(CastingSourceField.FieldType, "ShapeProvider"));
        shape.SetShape(vertices, indices, ShadowShape2D.OutlineTopology.Lines,
            ShadowShape2D.WindingOrder.CounterClockwise, false, true);
        _caster.castingOption = ShadowCaster2D.ShadowCastingOptions.CastShadow;
        _caster.Update();
    }

    private static void GetOutline(Mesh mesh, out Vector3[] points, out int[] edges)
    {
        if (mesh.vertexCount == 0)
        {
            points = Array.Empty<Vector3>();
            edges = Array.Empty<int>();
            return;
        }
        if (mesh.subMeshCount > 0 && mesh.GetTopology(0) == MeshTopology.Lines)
        {
            points = mesh.vertices;
            edges = mesh.GetIndices(0);
            return;
        }

        // Pre-upgrade baked shadows encode each directed outline edge in RGBA.
        // Rebuild their geometry through URP 17 instead of reusing the obsolete
        // render-mesh vertex layout. Existing map assets do not need rebaking.
        Color[] colors = mesh.colors;
        if (colors.Length != mesh.vertexCount)
            throw new InvalidOperationException("The chunk shadow mesh contains no supported outline data.");

        var unique = new HashSet<(Vector2, Vector2)>();
        var remaining = new List<(Vector2 start, Vector2 end)>();
        foreach (Color color in colors)
        {
            var start = new Vector2(color.r, color.g);
            var end = new Vector2(color.b, color.a);
            if (start != end && unique.Add((start, end)))
                remaining.Add((start, end));
        }

        var positions = new List<Vector3>();
        var lines = new List<int>();
        while (remaining.Count > 0)
        {
            var edge = remaining[0];
            remaining.RemoveAt(0);
            int first = positions.Count;
            positions.Add(edge.start);
            Vector2 current = edge.end;
            while (current != (Vector2)positions[first])
            {
                int previous = positions.Count - 1;
                positions.Add(current);
                lines.Add(previous);
                lines.Add(previous + 1);
                int next = remaining.FindIndex(candidate => candidate.start == current);
                if (next < 0)
                    throw new InvalidOperationException("The baked chunk shadow outline is not closed.");
                current = remaining[next].end;
                remaining.RemoveAt(next);
            }
            lines.Add(positions.Count - 1);
            lines.Add(first);
        }
        points = positions.ToArray();
        edges = lines.ToArray();
    }
}
