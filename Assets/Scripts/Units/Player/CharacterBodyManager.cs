using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;

public class CharacterBodyManager : MonoBehaviour
{
    [SerializeField] GameObject wholeBody;
    string category_hairFront = "FrontHair";
    string category_hairRear = "RearHair";

    [SerializeField] SpriteResolver hairFront;
    [SerializeField] SpriteResolver hairRear;
   public SpriteRenderer mainSprite;
    Animator characterAnimator;
    CharacterType myCharacter;
    float lastChangeTime = 0f;
    float changeDelay = 7f;


    private void Awake()
    {
        characterAnimator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        lastChangeTime = Time.time;
    }
    private void Update()
    {

        if (Time.time >= lastChangeTime + changeDelay)
        {
            ChangeAnimation();
            lastChangeTime = Time.time;
        }


    }

    public void SetCharacterSkin(CharacterType characterType) {
        myCharacter = characterType;
        SetHairSkins(characterType.ToString());
    }

    internal void SetHairSkins(string key)
    {
        hairFront.SetCategoryAndLabel(category_hairFront, key+"_FRONT");
        hairRear.SetCategoryAndLabel(category_hairRear, key+"_REAR");
        hairFront.ResolveSpriteToSpriteRenderer();
        hairRear.ResolveSpriteToSpriteRenderer();
    }

    int maxAnim = 5;
    string animKey = "animNumber";
    internal void ChangeAnimation()
    {
        int rand = Random.Range(0, maxAnim);
         characterAnimator.SetInteger(animKey,rand);

    }

}
