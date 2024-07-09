using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterCreatorElement : MonoBehaviour
{
    [Header("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")]
    [SerializeField] private SkinnedMeshRenderer _element;
    [Space(5)]
    [Header("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")]
    [SerializeField] private Transform _rootBone;
    [SerializeField] private BoneSet _boneSet;
    private void Start() => StartCoroutine(nameof(CreateElement));

    private IEnumerator CreateElement()
    {
        SkinnedMeshRenderer instance = Instantiate(_element, transform.root);
        instance.name = "CharacterBase";

        var tempBase = CharacterCreatorBaseInstance.Get(_boneSet);
        instance.bones = tempBase.GetBones();
        yield return new WaitForNextFrameUnit();
        tempBase.Release();

        instance.rootBone = _rootBone;
        instance.transform.SetParent(transform.parent);
    }
}
