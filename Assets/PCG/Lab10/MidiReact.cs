using System.Collections.Generic;
using MidiPlayerTK;
using TMPro;
using UnityEngine;

public class MidiReact : MonoBehaviour
{
    public MidiFilePlayer midiFilePlayer;
    public Canvas canvas;
    public MidiNote midiNotePrefab;
    public CelluralAutomataDigger celluralAutomataDigger;

    void Start() {
        if (midiFilePlayer == null) return;
        midiFilePlayer.OnEventNotesMidi.AddListener(OnEventNote);
    }

    void OnEventNote(List<MPTKEvent> notes) {
        int notesCount = 0;
        foreach (MPTKEvent ev in notes) {
            switch (ev.Command) {
                case MPTKCommand.NoteOn:
                    print(ev.Value);
                    // MidiNote note = Instantiate(midiNotePrefab, new(ev.Value * 20 - 650, 0, 0), Quaternion.identity, canvas.transform);
                    // note.GetComponent<TMP_Text>().text = ev.Value.ToString();
                    notesCount += 1;
                    if (celluralAutomataDigger.finished && notesCount % 10 == 0) {
                        celluralAutomataDigger.StartRandomDigger();
                    }
                    break;
            }
        }
    }

    void Update() {
        
    }
}
