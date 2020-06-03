﻿using System.Collections.Generic;
using UnityEngine;

public class ChartBase : MonoBehaviour
{

    public Material mat;//mesh材质
    /// <summary>
    /// 所有的左面坐标
    /// </summary>
    protected List<Vector3> leftPoints = new List<Vector3>();
    /// <summary>
    /// 所有的前面坐标
    /// </summary>
    protected List<Vector3> forwardPoints = new List<Vector3>();
    /// <summary>
    /// 所有的底面坐标
    /// </summary>
    protected List<Vector3> bottomPoints = new List<Vector3>();
    /// <summary>
    /// 所有的后面坐标
    /// </summary>
    protected List<Vector3> backPoints = new List<Vector3>();
    /// <summary>
    /// 所有的上面坐标
    /// </summary>
    protected List<Vector3> upPoints = new List<Vector3>();
    /// <summary>
    /// 所有的右面坐标
    /// </summary>
    protected List<Vector3> rightPoints = new List<Vector3>();

    /// <summary>
    /// 所有的顶点添加偏移
    /// </summary>
    protected List<PointsInfo> verticesAddOffset = new List<PointsInfo>();

    /// <summary>
    /// 索引
    /// </summary>
    protected List<int[]> triangles = new List<int[]>();

    /// <summary>
    /// 单个物体的点位信息
    /// </summary>
    [HideInInspector]
    public List<PointsInfo> pointsInfos = new List<PointsInfo>();

    protected List<Mesh> meshs = new List<Mesh>();

    /// <summary>
    /// 所有的生成的父物体
    /// </summary>
    protected List<GameObject> allChartParentObj = new List<GameObject>();
    /// <summary>
    /// 所有的生成的子物体
    /// </summary>
    protected List<GameObject> allChartItemObj = new List<GameObject>();

    /// <summary>
    /// 生成点位中心
    /// </summary>
    [Header("生成点位中心")]
    public Vector3 center = Vector3.zero;

    protected string meshName;

    public virtual void Awake()
    {
        SetMeshName();

    }
    public virtual void SetMeshName()
    {
        meshName = gameObject.name;
    }
    private void OnEnable()
    {
        CreateMesh();
        SetMeshInfo();
        ApplyValue();
        ShowAnim();
    }

    /// <summary>
    /// 展示动画
    /// </summary>
    public virtual void ShowAnim()
    {
        for (int i = 0; i < pointsInfos.Count; i++)
        {
            for (int j = 0; j < pointsInfos[i].points.Length; j++)
            {
                int tempI = i;
                int tempJ = j;

                StartCoroutine(DoFloatValue(0, pointsInfos[i].points[j].y, 1, (value) =>
                {
                    pointsInfos[tempI].points[tempJ].y = value;
                }));
            }
        }
    }


    protected  System.Collections.IEnumerator DoFloatValue(float startValue, float endValue, float time, System.Action<float> ChangeAction)
    {
        float tempF = startValue;
        float offsetValue = (endValue - startValue) / (time / 0.02f);
        bool addOrReduce = offsetValue >= 0;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            tempF += offsetValue;
            ChangeAction(tempF);
            if ((addOrReduce && tempF >= endValue) || (!addOrReduce && tempF <= endValue))
            {
                ChangeAction(endValue);
                break;
            }
        }

    }

    public virtual void Update()
    {
        CreateMesh();
        SetMeshInfo();
        ApplyValue();
    }

    /// <summary>
    /// 创建mesh
    /// </summary>
    public virtual void CreateMesh()
    {
        if (pointsInfos.Count != meshs.Count)
        {
            DeleteAllItemObjs();
            GameObject objParent = new GameObject();
            objParent.name = meshName;
            objParent.transform.position = Vector3.zero;
            allChartParentObj.Add(objParent);
            for (int i = 0; i < pointsInfos.Count; i++)
            {
                GameObject itemObj = new GameObject();
                itemObj.transform.parent = objParent.transform;
                itemObj.name = meshName+"_Child_"+i.ToString();
                Mesh mesh = new Mesh();
                meshs.Add(mesh);
                itemObj.AddComponent<MeshFilter>().mesh = mesh;
                itemObj.AddComponent<MeshRenderer>();
                itemObj.GetComponent<MeshRenderer>().material = mat;
                allChartItemObj.Add(itemObj);
            }
        }
    }

    protected void DeleteAllItemObjs()
    {
        for (int i = 0; i < allChartParentObj.Count; i++)
        {
            DestroyImmediate(allChartParentObj[i]);
        }
        allChartParentObj.Clear();
        allChartItemObj.Clear();
        meshs.Clear();
    }

    /// <summary>
    /// 设置顶点信息
    /// </summary>
    public virtual void SetMeshInfo()
    {
        triangles.Clear();
        verticesAddOffset.Clear();

        for (int i = 0; i < pointsInfos.Count; i++)
        {
            Vector3[] vertices = GetPointsVector3s(pointsInfos[i]);
            verticesAddOffset.Add(new PointsInfo(SetVerticesOffset(vertices, pointsInfos[i]), Vector3.one));
            triangles.Add(GetTrianglesVector3s(vertices));
        }

    }


    public virtual void ApplyValue()
    {
        for (int i = 0; i < meshs.Count; i++)
        {
            meshs[i].Clear();
            meshs[i].vertices = verticesAddOffset[i].points;
            meshs[i].triangles = triangles[i];
            meshs[i].RecalculateNormals();//重置法线
                                          //  meshs[i].normals = ResetNormals(meshs[i].vertices, meshs[i].normals);
            meshs[i].RecalculateBounds();   //重置范围
        }

    }

    //Vector3[] ResetNormals(Vector3[] vertices, Vector3[] normals)
    //{
    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        List<int> indexes = new List<int>();
    //        for (int j = 0; j < vertices.Length; j++)
    //        {
    //            if (vertices[i] == vertices[j] && i != j)
    //            {
    //                indexes.Add(i);
    //            }
    //        }
    //        Vector3 tempMiddle = Vector3.zero;
    //        for (int j = 0; j < indexes.Count; j++)
    //        {
    //            tempMiddle += normals[indexes[j]];
    //        }
    //        for (int j = 0; j < indexes.Count; j++)
    //        {
    //            normals[indexes[j]] = tempMiddle;
    //        }
    //    }

    //    return normals;
    //}



    /// <summary>
    /// 根据顶点和位移差获取最终位置
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual Vector3[] SetVerticesOffset(Vector3[] vertices, PointsInfo _points)
    {
        Vector3[] tempV3 = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            //Vector3 tempDir =Vector3.Normalize( vertices[i] - center);

            tempV3[i] = new Vector3(vertices[i].x, 0, vertices[i].z);

            int indexX = (int)vertices[i].x;

            if (_points.size.y == vertices[i].y)
            {
                tempV3[i] += new Vector3(0, _points.points[indexX].y, 0);
            }
            tempV3[i] += new Vector3(_points.points[indexX].x, 0, _points.points[indexX].z);

        }

        return tempV3;
    }



    /// <summary>
    /// 来自大佬的方法，求直线上的投影点
    /// </summary>
    /// <param name="P"> 直线外的点</param>
    /// <param name="A">直线上点</param>
    /// <param name="B">直线上点</param>
    /// <returns></returns>
    protected Vector3 LinePointProjection(Vector3 P, Vector3 A, Vector3 B)
    {
        Vector3 v = B - A;
        return A + v * (Vector3.Dot(v, P - A) / Vector3.Dot(v, v));
    }

    /// <summary>
    /// 获取正方体的每个面顶点坐标   (先生成最左面的第一竖列顶点，然后依次推导出后面的点)
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    protected virtual Vector3[] GetPointsVector3s(PointsInfo _points)
    {

        List<Vector3> tempPoints = new List<Vector3>();

        leftPoints.Clear();
        forwardPoints.Clear();
        bottomPoints.Clear();
        backPoints.Clear();
        upPoints.Clear();
        rightPoints.Clear();


        leftPoints.Add(new Vector3(0, 0, 0));//底面点
        leftPoints.Add(new Vector3(0, 0, _points.size.z));//底面点
        leftPoints.Add(new Vector3(0, _points.size.y, _points.size.z));
        leftPoints.Add(new Vector3(0, _points.size.y, 0));

        int midCount = _points.points.Length - 1;
        for (int i = 0; i < midCount; i++)
        {
            forwardPoints.Add(leftPoints[0] + new Vector3(_points.size.x * i, 0, 0));//底面点
            forwardPoints.Add(leftPoints[3] + new Vector3(_points.size.x * i, 0, 0));
            forwardPoints.Add(leftPoints[3] + new Vector3(_points.size.x * (i + 1), 0, 0));
            forwardPoints.Add(leftPoints[0] + new Vector3(_points.size.x * (i + 1), 0, 0));//底面点
        }

        for (int i = 0; i < midCount; i++)
        {
            bottomPoints.Add(leftPoints[1] + new Vector3(_points.size.x * i, 0, 0));//底面点
            bottomPoints.Add(leftPoints[0] + new Vector3(_points.size.x * i, 0, 0));//底面点
            bottomPoints.Add(leftPoints[0] + new Vector3(_points.size.x * (i + 1), 0, 0));//底面点
            bottomPoints.Add(leftPoints[1] + new Vector3(_points.size.x * (i + 1), 0, 0));//底面点
        }

        for (int i = 0; i < midCount; i++)
        {
            backPoints.Add(leftPoints[2] + new Vector3(_points.size.x * i, 0, 0));
            backPoints.Add(leftPoints[1] + new Vector3(_points.size.x * i, 0, 0));//底面点
            backPoints.Add(leftPoints[1] + new Vector3(_points.size.x * (i + 1), 0, 0));//底面点
            backPoints.Add(leftPoints[2] + new Vector3(_points.size.x * (i + 1), 0, 0));
        }

        for (int i = 0; i < midCount; i++)
        {
            upPoints.Add(leftPoints[3] + new Vector3(_points.size.x * i, 0, 0));
            upPoints.Add(leftPoints[2] + new Vector3(_points.size.x * i, 0, 0));
            upPoints.Add(leftPoints[2] + new Vector3(_points.size.x * (i + 1), 0, 0));
            upPoints.Add(leftPoints[3] + new Vector3(_points.size.x * (i + 1), 0, 0));
        }

        rightPoints.Add(leftPoints[3] + new Vector3(_points.size.x * midCount, 0, 0));
        rightPoints.Add(leftPoints[2] + new Vector3(_points.size.x * midCount, 0, 0));
        rightPoints.Add(leftPoints[1] + new Vector3(_points.size.x * midCount, 0, 0));//底面点
        rightPoints.Add(leftPoints[0] + new Vector3(_points.size.x * midCount, 0, 0));//底面点


        tempPoints.AddRange(leftPoints);
        tempPoints.AddRange(forwardPoints);
        tempPoints.AddRange(bottomPoints);
        tempPoints.AddRange(backPoints);
        tempPoints.AddRange(upPoints);
        tempPoints.AddRange(rightPoints);

        return tempPoints.ToArray();
    }

    /// <summary>
    /// 设置三角面索引
    /// </summary>
    /// <param name="vertices"></param>
    /// <returns></returns>
    protected int[] GetTrianglesVector3s(Vector3[] vertices)
    {
        List<int> all = new List<int>();
        for (int i = 0; i < vertices.Length; i++)
        {
            if (i % 4 == 0) //每个面四个顶点单独设置
            {
                SetIndex(all, i);
            }
        }
        return all.ToArray();
    }

    /// <summary>
    /// 通过每个面设置三角形索引
    /// </summary>
    /// <param name="ls"></param>
    /// <param name="i"></param>
    protected void SetIndex(List<int> ls, int i)
    {
        ls.Add(i);
        ls.Add(i + 1);
        ls.Add(i + 2);
        ls.Add(i);
        ls.Add(i + 2);
        ls.Add(i + 3);
    }
}



[System.Serializable]
public partial class PointsInfo
{
    public Vector3[] points;
    /// <summary>
    /// 三个轴向的尺寸
    /// </summary>
    public Vector3 size;
    public PointsInfo(Vector3[] _points, Vector3 _size)
    {
        points = _points;
        size = _size;
    }
}

[System.Serializable]
public class Indexes
{
    public int[] index;
    public Indexes(int[] _index)
    {
        index = _index;
    }
}