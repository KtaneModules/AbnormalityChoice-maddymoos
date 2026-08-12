using KModkit;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using wawa.DDL;
using Rnd = UnityEngine.Random;


public class abnochoice : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMSelectable ModSelectable;

    public KMSelectable[] AbnormalityBoxes;

    public AudioSource AbnoChoiceTheme;
    private float ambVol;
    private bool Focused;

    public TextMesh[] AbnoIDs;
    public TextMesh DayNum;
    public Transform[] AbnoBoxes;
    public Sprite[] AbnoBoxesTex;
    public SpriteRenderer[] AbnoBoxesRenderer;

    public SpriteRenderer darkfilter;
    public SpriteRenderer fadeinFilter;

    public TextAsset abnoList;

    public Transform[] stats;

    public Transform Hide;

    public MeshRenderer flavorMat;
    public Text flavortext;

    static int moduleIdCounter = 1;
    int moduleId;

    private int[] defStats = new int[4];
    private int[] workStats = new int[4];
    private int daynum;

    private bool[] biggify = new bool[3];
    private float[] bigamt = new float[3];
    private bool dark;
    private bool animating;
    private float howDark;
    private float flavorNum;

    private bool struck = false, autosolved = false, solved = false, focuslock, hold;


    private float[] boxSway = new float[3];
    private bool[] boxDir = new bool[3];
    private bool[] fastSway = new bool[3];

    private int[] abnonums = new int[3];
    private float[] abnoscore = new float[3];

    static private string[] HazardOrder = { "Zayin", "Teth", "He", "Waw", "Aleph" };
    static private string[] ColorOrder = { "RED", "WHITE", "BLACK", "PALE" };
    static private string[] Ordianals = { "first", "second", "third" };
    static private string[] WorkDefense = { "vulnerable to","weak to","normal against","enduring to", "resistant to", "immune to" };
    static private string[] RomanNum = { "I", "II", "III", "IV", "V" };
    static private string[] worktypes = { "Instinct", "Insight", "Attachment", "Repression" };

    private int[][] DayWeights = new int[][]
    {
        new int[] {5,3,0,0,0},
        new int[] {3,5,3,0,0 },
        new int[] {1,4,5,3,0 },
        new int[] {0,3,5,4,1 },
        new int[] {0,2,4,5,2 },
        new int[] {0,1,3,4,3 },
        new int[] {0,0,2,3,4 },
        new int[] {0,0,1,2,5 }
    };

    private int[][] DefWeights = new int[][]
    {
        new int[]{-1,0,1,2,3,4},
        new int[]{-2,-1,0,1,2,3 },
        new int[]{-3,-2,-1,1,3,5 },
        new int[]{-4,-3,-2,0,2,4 },
        new int[]{-6,-4,-2,0,3,6 }
    };

    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < 3; i++)
        {
            int k = i;
            AbnormalityBoxes[k].OnHighlight += delegate ()
            {
                if(hold)
                {
                    AbnormalityBoxes[k].OnHighlightEnded();
                    return;
                }
                if (!animating)
                {
                    Audio.PlaySoundAtTransform("DoorOn", AbnormalityBoxes[k].transform);
                AbnoBoxesRenderer[k].sprite = AbnoBoxesTex[1];
                AbnoBoxesRenderer[k].sortingOrder = -1;
                
                    biggify[k] = true;
                    dark = true;
                }
            };
            AbnormalityBoxes[k].OnInteract += delegate ()
            {
                if (!animating)
                {
                    CheckAnswer(k);
                }
                return false;
            };
            AbnormalityBoxes[k].OnHighlightEnded += delegate ()
            {
                if (!animating)
                {
                    AbnoBoxesRenderer[k].sprite = AbnoBoxesTex[0];
                    AbnoBoxesRenderer[k].sortingOrder = -3;
                    biggify[k] = false;
                    dark = false;
                }
            };
        }
        ModSelectable.OnFocus += delegate ()
        {
            StartCoroutine(Hold());
            if(!AbnoChoiceTheme.isPlaying)
            {
                StartCoroutine(FadeIn());
                AbnoChoiceTheme.Play();
            }
            if (!focuslock)
            Focused = true;
        };
        ModSelectable.OnDefocus += delegate ()
        {
            if (!focuslock)
                Focused = false;
        };

    }

    IEnumerator Hold()
    {
        hold = true;
        yield return new WaitForSeconds(0.5f);
        hold = false;
    }

    // Use this for initialization
    void Start () {
        fadeinFilter.color = new Color(0, 0, 0, 1);
        for (int i = 0;i<3;i++)
        {
            boxSway[i] = Rnd.Range(0, 100) / 100f;
            if (Rnd.Range(0, 2) == 0) boxDir[i] = true;
        }
        GenerateStats();
        GenerateAbnos();
        if (!Application.isEditor)
        {
            return;
        }
        int[] type = new int[5];
        foreach(Abnormality ay in AbnoList)
        {
            type[Array.IndexOf(HazardOrder, ay.Class)]++;
        }
        Debug.LogFormat("{0} {1} {2} {3} {4}", type[0], type[1], type[2], type[3], type[4]);

    }

    void CheckAnswer(int j)
    {
        if (abnoscore[j] == abnoscore.Max())
        {
            focuslock = true;
            solved = true;
            if(!struck && !autosolved)
            {
                Debug.LogFormat("[Abnormality Choice #{0}]: Good choice, Manager. I knew you'd choose well.", moduleId);
            }
            else if(!autosolved)
            {
                Debug.LogFormat("[Abnormality Choice #{0}]: Good choice, Manager. Your insticts are off.", moduleId);
            }
                animating = true;

            StartCoroutine(SlideContainerAndFade(j));
        }
        else
        {
            biggify[j] = true;
            dark = true;
            struck = true;
            Module.HandleStrike();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, Module.transform);
            if (abnoscore[j] > 0)
            {
                Debug.LogFormat("[Abnormality Choice #{0}]: No, Manager! {1} isn't the best choice. We can do better.", moduleId, AbnoList[abnonums[j]].Name);
                Audio.PlaySoundAtTransform("Bad" + AbnoList[abnonums[j]].Class, Module.transform);
            }
            else
            {
                Debug.LogFormat("[Abnormality Choice #{0}]: No, Manager! {1} is a horrible choice! It will put our facility at risk.", moduleId, AbnoList[abnonums[j]].Name);
                Audio.PlaySoundAtTransform("VeryBad" + AbnoList[abnonums[j]].Class, Module.transform);
            }
        }
    }

    IEnumerator SlideContainerAndFade(int index)
    {
        yield return null;
        Audio.PlaySoundAtTransform("DoorClick", Module.transform);
        float i = 0;
        while (i < 1)
        {
            if(i < .75f)
            AbnoBoxes[index].localPosition = Vector3.Lerp(new Vector3(AbnoBoxes[index].localPosition.x, 0.014f, -.0309f), new Vector3(AbnoBoxes[index].localPosition.x, .014f, -.2f), Mathf.Pow(i, 2));
            if (i > .67)
            {
                AbnoIDs[index].text = "";
            }
            darkfilter.color = Color.Lerp(new Color(1, 1, 1, .5f), new Color(1, 1, 1, 1), i);
            i += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(.6f);
        Module.HandlePass();
        StartCoroutine(FadeOut());
    }
    void GenerateStats()
    {
        Debug.LogFormat("[Abnormality Choice #{0}]: It's time to choose a new Abnormality, Manager.", moduleId);
        do
        {
            daynum = Rnd.Range(2, 40);
            DayNum.text = "Day " + daynum;
        }
        while (daynum % 5 == 0);
        Debug.LogFormat("[Abnormality Choice #{0}]: Today is Day {1}.", moduleId, daynum);
        for (int i=0;i<4;i++)
        {
            stats[i].GetComponent<MeshRenderer>().enabled = true;
            stats[i+4].GetComponent<MeshRenderer>().enabled = true;

            if (daynum > 30)
            {
                defStats[i] = Rnd.Range(2, 6);
                workStats[i] = Rnd.Range(2, 5);
            }
            else if (daynum > 13)
            {
                defStats[i] = Rnd.Range(0, 6);
                workStats[i] = Rnd.Range(0, 5);
            }
            else
            {
                defStats[i] = Rnd.Range(0, 4);
                workStats[i] = Rnd.Range(0, 3);
            }
                stats[i].localScale = Vector3.Lerp(new Vector3(0, 1, .825f), new Vector3(.95f, 1, .825f), (float)defStats[i] / 5);
            stats[i].localPosition = Vector3.Lerp(new Vector3(-.475f, .06f, 0), new Vector3(0, .06f, 0), (float)defStats[i] / 5);
            stats[i+4].localScale = Vector3.Lerp(new Vector3(0, 1, .825f), new Vector3(.95f, 1, .825f), (float)workStats[i] / 4);
            stats[i+4].localPosition = Vector3.Lerp(new Vector3(-.475f, .06f, 0), new Vector3(0, .06f, 0), (float)workStats[i] / 4);
            if (defStats[i] == 0)
            {
                stats[i].GetComponent<MeshRenderer>().enabled = false;
            }
            if (workStats[i] == 0)
            {
                stats[i+4].GetComponent<MeshRenderer>().enabled = false;
            }
            Debug.LogFormat("[Abnormality Choice #{0}]: We are {1} {2} damage.", moduleId, WorkDefense[defStats[i]], ColorOrder[i]);
            Debug.LogFormat("[Abnormality Choice #{0}]: Our employees are Level {1} at {2} work.", moduleId, RomanNum[workStats[i]], worktypes[i]);
        }
    }
    internal static List<Abnormality> AbnoList = new List<Abnormality>();

    void GenerateAbnos()
    {
        GetAbnoList();
        do
        {
            do
            {
                for (int i = 0; i < abnonums.Length; i++)
                {
                    do
                    {
                        abnonums[i] = Rnd.Range(0, AbnoList.Count());
                        AbnoIDs[i].text = AbnoList[abnonums[i]].ID;
                    } while (daynum < 13 ? (Array.IndexOf(HazardOrder, AbnoList[abnonums[i]].Class) > 2) : false);
                }
            } while (abnonums[0] == abnonums[1] || abnonums[0] == abnonums[2] || abnonums[1] == abnonums[2]);
            // Score Calc

            for (int i = 0; i < 3; i++)
            {

                abnoscore[i] = 0;
                
                //Day Bias
                abnoscore[i] += DayWeights[daynum / 5][Array.IndexOf(HazardOrder, AbnoList[abnonums[i]].Class)];

                Debug.Log(abnoscore[i]);
                //Damage Bias
                abnoscore[i] += DefWeights[Array.IndexOf(HazardOrder, AbnoList[abnonums[i]].Class)]
                    [defStats[Array.IndexOf(ColorOrder, AbnoList[abnonums[i]].dmgColor)]];

                Debug.Log(abnoscore[i]);
                //WorkSkill

                abnoscore[i] = abnoscore[i] * AbnoList[abnonums[i]].workMult[workStats[Array.IndexOf(ColorOrder, AbnoList[abnonums[i]].perfWork)]];


            }
        }
        while (abnoscore[0] == abnoscore[1] || abnoscore[1] == abnoscore[2] || abnoscore[0] == abnoscore[2]);
        for (int i = 0; i < 3; i++)
        Debug.LogFormat("[Abnormality Choice #{0}] For our {1} choice, we have {2}. It is {6} {3} Abnormality that deals {4} damage and perfers {5} work.", moduleId, Ordianals[i], AbnoList[abnonums[i]].Name, AbnoList[abnonums[i]].Class, AbnoList[abnonums[i]].dmgColor, worktypes[Array.IndexOf(ColorOrder,AbnoList[abnonums[i]].perfWork)], AbnoList[abnonums[i]].Class == "Aleph" ? "an" : "a");
        for (int i=0;i<3; i++)
        Debug.LogFormat("[Abnormality Choice #{0}] {1}'s final score is {2}.", moduleId, AbnoList[abnonums[i]].Name, abnoscore[i]);
        int maxindex = Array.IndexOf(abnoscore, abnoscore.Max());
        Debug.LogFormat("[Abnormality Choice #{0}] Manager, you should choose {1}.", moduleId, AbnoList[abnonums[maxindex]].Name);
    }
    void GetAbnoList()
    {
        AbnoList = JsonConvert.DeserializeObject<List<Abnormality>>(abnoList.text);
        Debug.Log(AbnoList[0].Name);
    }

    IEnumerator FadeIn()
    {
        float i = 0;
        fadeinFilter.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), i);
        while(i<1)
        {
            yield return null;
            fadeinFilter.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), i);
            i += Time.deltaTime / 3;
        }
        fadeinFilter.enabled = false;
    }
    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(2f);
        fadeinFilter.enabled = true;
        float i = 0;
        while (i < 1)
        {
            yield return null;
            fadeinFilter.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), i);
            ambVol = 1 - i;
            i += Time.deltaTime / 3;
        }
        Hide.localPosition = Vector3.zero;
        Hide.localScale = Vector3.zero;
        Focused = false;
        
    }

    // Update is called once per frame
    void Update () {
        if (!solved && (biggify[0] || biggify[1] || biggify[2]))
        {
            flavorNum += Time.deltaTime * 10;
            if (flavorNum > 1) flavorNum = 1;
        }
        else
        {
            flavorNum -= Time.deltaTime * 10;
            if (flavorNum < 0) flavorNum = 0;
        }
            flavortext.color = new Color(1, 0, 0, flavorNum);
        flavorMat.material.color = new Color(1, 1, 1, flavorNum);
        for (int i = 0; i < 3; i++)
        {
            if (biggify[i])
            {
                flavortext.text = AbnoList[abnonums[i]].tagline;
                bigamt[i] += Time.deltaTime * 20;
                if (bigamt[i] > 1) bigamt[i] = 1;
            }
            else
            {
                bigamt[i] -= Time.deltaTime * 20;
                if (bigamt[i] < 0) bigamt[i] = 0;
            }
            
            AbnoBoxes[i].localScale = Vector3.Lerp(new Vector3(.045f, .005f, .045f), new Vector3(.055f, .005f, .055f), bigamt[i]);
            AbnoBoxes[i].localEulerAngles = Vector3.Lerp(new Vector3(0, -2, 0), new Vector3(0, 2, 0), easeInOutCubic(boxSway[i]));
            if (boxDir[i])
            {
                boxSway[i] += Time.deltaTime / 4 * (biggify[i] ? 15 : 1) * (fastSway[i] ? 10 : 1);
                if (boxSway[i] > 1)
                {
                    boxSway[i] = 1;
                    boxDir[i] = false;
                    if(Rnd.Range(0,3) == 0)
                    {
                        fastSway[i] = true;
                    }
                    else
                    {
                        fastSway[i] = false;
                    }
                }
                
            }
            else
            {
                boxSway[i] -= Time.deltaTime / 4 * (biggify[i] ? 15 : 1) * (fastSway[i] ? 10 : 1);
                if (boxSway[i] <0)
                {
                    boxSway[i] = 0;
                    boxDir[i] = true;
                }
            }
        }
        if (!Focused)
        {
            ambVol -= .1f;
            if (ambVol < 0)
                ambVol = 0;
        }
        else
        {
            ambVol += .01f;
            if (ambVol > 1)
                ambVol = 1;
        }
        AbnoChoiceTheme.volume = (wawa.DDL.Preferences.Sound / 500f) * ambVol;
        if(dark && !animating)
        {
            howDark += Time.deltaTime * 10;
            if (howDark > 1) howDark = 1;
            darkfilter.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, .5f), howDark);

        }
        else if (!animating)
        {
            howDark -= Time.deltaTime * 25;
            if (howDark < 0) howDark = 0;
            darkfilter.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, .5f), howDark);
        }
    }

    float easeInOutCubic(float x) {
    return x< 0.5 ? 4 * x* x* x : 1 - (float)Math.Pow(-2 * x + 2, (double)3) / 2;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} cycle (cycles across the Abnormalities) / !{0} left/middle/right (selects the left/middle/right Abnormality)";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;
        if ((m = Regex.Match(command, @"^\s*((cycle)|(left)|(middle)|(right))$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            yield return null;
            var input = m.Groups[1].Value.ToLowerInvariant();
            switch (input)
            {
                case "cycle":
                    for (int i = 0; i < 3; i++)
                    {
                        AbnormalityBoxes[i].OnHighlight();
                        yield return new WaitForSeconds(1.5f);
                        AbnormalityBoxes[i].OnHighlightEnded();
                        yield return new WaitForSeconds(.25f);
                    }
                    break;
                case "left":
                    AbnormalityBoxes[0].OnInteract();
                    break;
                    case "right":
                    AbnormalityBoxes[2].OnInteract();
                    break;
                case "middle":
                    AbnormalityBoxes[1].OnInteract();
                    break;
            }
        }
        else
        {
            yield return "sendtochaterror Unknown command. Use either cycle, left, middle, or right..";
            yield break;
        }
    }
    private IEnumerator TwitchHandleForcedSolve()
    {
        ModSelectable.OnFocus();
        while(fadeinFilter.enabled)
            yield return true;
        if(solved)
        {
            Debug.LogFormat("[Abnormality Choice #{0}]: Sigh, Manager. Fine, I'll pick for you.", moduleId);
        }    
        autosolved = true;
        AbnormalityBoxes[Array.IndexOf(abnoscore,abnoscore.Max())].OnInteract();
        yield return new WaitForSeconds(.1f);
    }
}

public class Abnormality
{
    public string Name;
    public string Class;
    public string ID;
    public string dmgColor;
    public string perfWork;
    public float[] workMult;
    public string tagline;
}

