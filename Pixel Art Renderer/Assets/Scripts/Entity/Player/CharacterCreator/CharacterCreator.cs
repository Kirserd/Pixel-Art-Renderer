using UnityEngine;

public struct CharacterCreatorBaseInstance
{
    public readonly GameObject Base;
    public readonly SkinnedMeshRenderer Skin;
    public Transform[] GetBones() => Skin.bones;
    public Vector3 GetInitPosition() => Skin.transform.localPosition;

    public CharacterCreatorBaseInstance(GameObject @base) 
    { 
        Base = @base; 
        Skin = Base.transform.GetChild(1)
            .GetComponent<SkinnedMeshRenderer>();
    }
    public static CharacterCreatorBaseInstance Get(BoneSet boneSet) 
        => new(CharacterCreator.Instance.GetBaseInstance(boneSet));
    public void Release() 
        => CharacterCreator.ReleaseInstance(this);
}
public class CharacterCreator : MonoBehaviour
{
    public static CharacterCreator Instance;

    #region Armature Binding
    [SerializeField] 
    private SerializableDictionary<BoneSet, GameObject> _basesLookUps;
    public GameObject GetBaseInstance(BoneSet boneSet)
    {
        var lookUp = _basesLookUps[boneSet];
        return Instantiate(lookUp, lookUp.transform);
    }
    public static void ReleaseInstance(CharacterCreatorBaseInstance instance) 
        => Destroy(instance.Base);
    #endregion

    private void Awake()
    {
        SetupDDOL();
        DontDestroyOnLoad(this);
    }
    private void SetupDDOL()
    {
        if (Instance is not null)
            Destroy(this);

        Instance = this;
    }
}
