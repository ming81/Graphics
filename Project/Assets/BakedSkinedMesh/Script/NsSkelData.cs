﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct _BoneData {
    // 根节点为-1
    public int parentBone;
    // 相对于父节点的偏移
    public Vector3 initOffset;
    // 相对于父节点的缩放
    public Vector3 initScale;
    // 相对于父节点的旋转
    public Quaternion initRot;
    // 姿势绑定，在T姿势下，从模型空间到骨骼空间的转换
    public Matrix4x4 bindPose;
    [NonSerialized]
    private Matrix4x4 initWorldMatrix;
    [NonSerialized]
    private bool m_WordMatrixNoDirty;

    // 相对于父节点
    public Matrix4x4 GetInitLocalTransMatrix() {
        Matrix4x4 ret = Matrix4x4.TRS(initOffset, initRot, initScale);
        return ret;
    }

    private Matrix4x4 GetParentInitWorldMatrix(_SkeletonData skl) {
        if (skl == null || parentBone < 0 || skl.m_BoneDatas  == null || parentBone >= skl.m_BoneDatas.Length)
            return Matrix4x4.identity;
        var pBone = skl.m_BoneDatas[parentBone];
        Matrix4x4 ret = pBone.GetInitGlobalTransMatrix(skl);
        return ret;
    }

    public Matrix4x4 GetInitGlobalParentTransMatrix(_SkeletonData skl) {
        if (parentBone < 0)
            return Matrix4x4.identity;
        if (skl != null && skl.m_BoneDatas != null && parentBone < skl.m_BoneDatas.Length) {
            var parent = skl.m_BoneDatas[parentBone];
            return parent.GetInitGlobalTransMatrix(skl);
        } else
            return Matrix4x4.identity;
    }

    public Matrix4x4 GetInitLocalParentTransMatrix(_SkeletonData skl) {
        if (parentBone < 0)
            return Matrix4x4.identity;
        if (skl != null && skl.m_BoneDatas != null && parentBone < skl.m_BoneDatas.Length) {
            var parent = skl.m_BoneDatas[parentBone];
            return parent.GetInitLocalTransMatrix();
        } else
            return Matrix4x4.identity;
    }


    public Matrix4x4 GetInitGlobalTransMatrix(_SkeletonData skl) {
        if (skl == null)
            return GetInitLocalTransMatrix();

        if (m_WordMatrixNoDirty) {
            return initWorldMatrix;
        }

        if (parentBone < 0)
            return GetInitLocalTransMatrix();

        initWorldMatrix = GetParentInitWorldMatrix(skl) * GetInitLocalTransMatrix();

        m_WordMatrixNoDirty = true;

        return initWorldMatrix;
    }

    public Vector3 GetInitBoneCenter(_SkeletonData skl, Matrix4x4 root) {
        Matrix4x4 mat = root * GetInitGlobalTransMatrix(skl);
        Vector3 ret = mat.GetColumn(3);
        return ret;
    }

}

[Serializable]
public struct _VertexBoneData {
    public int boneIndex1;
    public int boneIndex2;
    public int boneIndex3;
    public int boneIndex4;

    public float boneWeight1;
    public float boneWeight2;
    public float boneWeight3;
    public float boneWeight4;
}

[Serializable]
public class _SkeletonData : ScriptableObject {
    public int m_StartBoneUV = -1;
    public int m_RootBoneIndex = -1;
    public _BoneData[] m_BoneDatas = null;
    public AnimationClip m_AnimClip = null;
    // 再存一份
    public _VertexBoneData[] m_VertexBoneData = null;
    public Matrix4x4[] bindPoseArray {
        get {
            Matrix4x4[] ret = null;
            if (m_BoneDatas == null || m_BoneDatas.Length <= 0)
                return ret;
            ret = new Matrix4x4[m_BoneDatas.Length];
            for (int i = 0; i < m_BoneDatas.Length; ++i) {
                ret[i] = m_BoneDatas[i].bindPose;
            }
            return ret;
        }
    }

    public List<Vector4> BoneIndexList {
        get {
            List<Vector4> ret = new List<Vector4>();
            if (m_VertexBoneData != null) {
                for (int  i = 0; i < m_VertexBoneData.Length; ++i) {
                    var data = m_VertexBoneData[i];
                    Vector4 vec = new Vector4(data.boneIndex1, data.boneIndex2, data.boneIndex3, data.boneIndex4);
                    ret.Add(vec);
                }
            }
            return ret;
        }
    }

    public List<Vector4> BoneWeightList {
        get {
            List<Vector4> ret = new List<Vector4>();
            if (m_VertexBoneData != null) {
                for (int i = 0; i < m_VertexBoneData.Length; ++i) {
                    var data = m_VertexBoneData[i];
                    Vector4 vec = new Vector4(data.boneWeight1, data.boneWeight2, data.boneWeight3, data.boneWeight4);
                    ret.Add(vec);
                }
            }
            return ret;
         }
    }
}
