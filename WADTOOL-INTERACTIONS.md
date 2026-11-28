# WadTool: interazioni legacy e equivalente WPF

Inventario ricavato dal codice legacy (`Tomb-Editor-develop/WadTool/Forms/Form*.cs` e `Controls/PanelRendering*.cs`) e confrontato con il codice WPF (`WadTool/Features/Dialogs/*`, `WadTool/Controls/*`). Stato: OK = presente nel WPF, FIX = mancava ed e' stato corretto, DOCK = cambiato dal passaggio ai pannelli dockable.

## Animation Editor (FormAnimationEditor.ProcessCmdKey)

Con il focus in un controllo che accetta testo i tasti singoli non agiscono (legacy: WinFormsUtils.CurrentControlSupportsInput).

| Input | Legacy | WPF |
| --- | --- | --- |
| Esc | deseleziona mesh, griglia bone, selezione timeline | OK (ClearSelections) |
| Sinistra / Destra | frame precedente / successivo con loop | OK (StepFrameLooping) |
| Su / Giu | primo / ultimo frame | OK |
| Spazio | play/stop | OK |
| I / O | inizio / fine selezione timeline | OK |
| Canc | elimina frame | OK |
| Ctrl+A / X / C / V | seleziona tutto, taglia, copia, incolla frame | OK |
| Ctrl+I | interpola | OK |
| Ctrl+S / Z / Y | salva, undo, redo | OK (attivi anche con testo) |
| Invio in Name / State | conferma | OK |
| Tasto destro doppio click sulla viewport | reset camera | OK |
| Tasto destro sulla timeline | menu contestuale | OK |
| Doppio click su anim command nella timeline | apre Anim commands | OK |
| Doppio click su riga bone | inverte il flag Use | OK |
| Mouse enter sulla viewport | azzera MadeChanges (undo) | OK (ResetUndoCoalescing) |
| Rotella / drag destro / drag centrale / Shift / Ctrl nella viewport | zoom, orbita, pan | OK (stesso pannello) |
| Rotella sul combo State chiuso | scorre gli stati | OK |
| Tutti i tasti con il focus nei pannelli dockable | n/a | DOCK: verificato (frecce, Spazio, Giu dalla viewport e dalla lista) |

## Altre finestre

| Finestra | Interazione legacy | WPF |
| --- | --- | --- |
| Main | Canc sull'albero destinazione elimina; doppio click sull'albero sorgente copia (Alt = slot diverso) | OK |
| Mesh Editor | Esc, Ctrl+Z/Y, Ctrl+F (solo modalita' facce), - + ~ \ ruota texture, Shift+quei tasti specchia | OK |
| Mesh Editor | Invio in ricerca mesh e in numero vertice; doppio click sulla lista mesh; click sul quadrato colore apre il selettore | OK |
| Skeleton Editor | Ctrl+Su/Giu sposta bone, Ctrl+O pop, Ctrl+P push; menu contestuale sull'albero con tasto destro; doppio click viewport reset camera | OK |
| Static Editor | Esc deseleziona luce, Canc elimina luce selezionata, doppio click viewport reset camera | OK |
| Anim Commands Editor | Canc elimina il comando | FIX (KeyBinding Canc sulla griglia) |
| State Changes Editor | doppio click cella riproduce (non sulla colonna curva); Invio=OK, Esc=Annulla | OK |
| Select Slot | doppio click conferma | OK |
| Blend Curve, Lua Properties, New Wad2, Options, Animation Fixer | Invio=OK, Esc=Annulla | OK |
| Replace Anim Commands | pulsante di chiusura e' anche Annulla | OK |
| Sprite Sequence Editor | nessun AcceptButton/CancelButton legacy | OK |
| Tutte | nessun drag and drop di file o oggetti nel legacy | OK (nessuno nel WPF) |
