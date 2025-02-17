using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;
[ExecuteInEditMode]
public class PoissonManager: MonoBehaviour
{
    public float Width = 10f, Height = 10f;
    [Range(0.1f,1f)]
    public float Radius = 2f;
    [Range(2, 10)]

    public int Iterations = 5 ;
    List<Vector2> samples = new List<Vector2>();

    PoissonDiskSampler sampler;
    private void OnEnable()
    {
        //samples =  sampler.PoissonSamples(Radius, Iterations, new Vector2(Width, Height));
        sampler = new PoissonDiskSampler();
    }
   // private void Update()
   // {
   //     samples = sampler.PoissonSamples(Radius, Iterations, new Vector2(Width, Height));
   // }
    [Button]
    public void DrawPointAnimated() 
    {
        sampler.AnimatedDrawDisk(Radius, Iterations, new Vector2(Width, Height));
    }
    private void OnDrawGizmos()
    {
        if (sampler.Allpoints == null)
            return;
        foreach (Vector2 v in sampler.Allpoints) 
        {
            Gizmos.DrawSphere(v, 0.1f);
            //Gizmos.(v, Radius);
        }
        Gizmos.color = Color.red;
        foreach (Vector2 v in sampler.ActivePoints)
        {
            Gizmos.DrawSphere(v, 0.1f);
            //Gizmos.(v, Radius);
        }
    }


}
