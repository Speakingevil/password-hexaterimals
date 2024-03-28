using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PasswordHexascript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> spinners;
    public Transform[] spinpos;
    public TextMesh[] displays;

    private readonly string alph = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly int[][] lines = new int[15][] { new int[3] { 0, 3, 7 }, new int[4] { 1, 4, 8, 12 }, new int[5] { 2, 5, 9, 13, 16 }, new int[4] { 6, 10, 14, 17 }, new int[3] { 11, 15, 18 }, new int[3] { 2, 1, 0 }, new int[4] { 6, 5, 4, 3 }, new int[5] { 11, 10, 9, 8, 7 }, new int[4] { 15, 14, 13, 12 }, new int[3] { 18, 17, 16 }, new int[3] { 11, 6, 2 }, new int[4] { 15, 10, 5, 1 }, new int[5] { 18, 14, 9, 4, 0 }, new int[4] { 17, 13, 8, 3 }, new int[3] { 16, 12, 7 } };
    private string[][] letters = new string[19][];
    private int[] spelled = new int[15];
    private int[] spins = new int[19];
    private bool spin;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        for (int i = 0; i < 19; i++)
            letters[i] = new string[6];
        string[] wlog = new string[15];
        for (int i = 0; i < 15; i++)
        {
            int line = lines[i].Length;
            string word = Wordlist.words[line - 3].PickRandom();
            wlog[i] = word;
            if (Random.Range(0, 2) == 1)
                word = new string(word.Reverse().ToArray());
            for (int j = 0; j < line; j++)
            {
                letters[lines[i][j]][i / 5] = word[j * 2].ToString();
                letters[lines[i][j]][(i / 5) + 3] = word[(j * 2) + 1].ToString();
            }
        }
        for (int i = 0; i < 3; i++)
            Debug.LogFormat("[Password Hexaterminals #{0}] Write the words: {1} & {2} along the {3}-axis.", moduleID, string.Join(", ", Enumerable.Range(i * 5, 4).Select(x => wlog[x]).ToArray()), wlog[(i * 5) + 4], "SRQ"[i]);
        int[] g = new int[19];
        do
        {
            for (int i = 0; i < 19; i++)
            {
                g[i] = Random.Range(0, 6);
                letters[i] = Cycle(i, -g[i]);
                for (int j = 0; j < 6; j++)
                    displays[(6 * i) + j].text = letters[i][j];
            }
            for (int i = 0; i < 15; i++)
                Check(i);
        } while (spelled.All(x => x != 0));
        Debug.Log(string.Join(", ", g.Select(x => x.ToString()).ToArray()));
        foreach(KMSelectable spinner in spinners)
        {
            int b = spinners.IndexOf(spinner);
            spinner.OnInteract += delegate ()
            {
                if(!moduleSolved && !spin)
                    StartCoroutine("Rotate", b);
                return false;
            };
        }
        StartCoroutine("Flicker");
    }

    private string[] Cycle(int x, int r)
    {
        string y = string.Join("", letters[x]);
        string[] z = new string[6];
        for (int i = 0; i < 6; i++)
            z[i] = alph[(y[(i + 6 + r) % 6] - 'A' + 26 + r) % 26].ToString();
        return z;
    }

    private IEnumerator Flicker()
    {
        while (!moduleSolved)
        {
            if (!spin)
            {
                for (int i = 0; i < 15; i++)
                {
                    int[] line = lines[i];
                    if (spelled[i] != 0)
                        for (int j = 0; j < line.Length; j++)
                        {
                            int x = line[j];
                            displays[(x * 6) + ((i / 5) % 6)].text = spelled[i] > 0 ? "\u25b2" : "\u25bc";
                            displays[(x * 6) + (((i / 5) + 3) % 6)].text = spelled[i] < 0 ? "\u25b2" : "\u25bc";
                        }
                }
                if(spelled.All(x => x != 0))
                {
                    moduleSolved = true;
                    module.HandlePass();
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    yield break;
                }
            }
            yield return new WaitForSeconds(1);
            for (int i = 0; i < 19; i++)
                for (int j = 0; j < 6; j++)
                    displays[(6 * i) + j].text = letters[i][j];
            yield return new WaitForSeconds(1);
        }
    }

    private void Check(int line)
    {
        string z = "";
        int[] li = lines[line];
        for(int i = 0; i < li.Length; i++)
        {
            int j = li[i];
            z += letters[j][line / 5];
            z += letters[j][(line / 5) + 3];
        }
        if (Wordlist.words[li.Length - 3].Contains(z))
            spelled[line] = 1;
        else
        {
            z = new string(z.Reverse().ToArray());
            if (Wordlist.words[li.Length - 3].Contains(z))
                spelled[line] = -1;
            else
                spelled[line] = 0;
        }
    }

    private IEnumerator Rotate(int b)
    {
        spin = true;
        int a = spins[b];
        if (spins[b] < 5)
        {
            Audio.PlaySoundAtTransform("Spinup", spinpos[b]);
            spins[b]++;
            letters[b] = Cycle(b, 1);
        }
        else
        {
            Audio.PlaySoundAtTransform("Spindown", spinpos[b]);
            spins[b] = 0;
            letters[b] = Cycle(b, -5);
        }
        float e = 0;
        while(e < 0.2f)
        {
            e += Time.deltaTime;
            float r = Mathf.Lerp(a / 5f, spins[b] / 5f, e * 5);
            float s = -e * 300;
            spinpos[b].localEulerAngles = new Vector3(-90, 0, s);
            for(int i = 0; i < 6; i++)
            {
                int c = (b * 6) + i;
                displays[c].color = new Color(r, 1, 0);
                displays[c].text = alph.PickRandom().ToString();
            }
            yield return null;
        }
        spinpos[b].localEulerAngles = new Vector3(-90, 0, 0);
        for (int i = 0; i < 6; i++)
        {
            int c = (b * 6) + i;
            displays[c].color = new Color(spins[b] / 5f, 1, 0);
            displays[c].text = letters[b][i];
        }
        for (int i = 0; i < 15; i++)
            if (lines[i].Contains(b))
                Check(i);
        spin = false;
    }
}
