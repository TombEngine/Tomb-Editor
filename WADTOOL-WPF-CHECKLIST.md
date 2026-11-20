# WadTool WPF: piano di lavoro autonomo

Obiettivo: portare il port MVVM di WadTool a parita' completa (aspetto e comportamento) con la versione WinForms legacy, e portare l'Animation Editor sui pannelli dockable come in Tomb Editor.

Riferimenti:
- Legacy: worktree `D:\Repositories\Tomb-Editor-develop` (WadTool/Forms/Form*.cs), mai modificato.
- Nuovo: `WadTool/Features/Dialogs/*` e `WadTool/Features/Main`.
- Metodo e dimensioni di test D1-D9: `WPF-PARITY-TEST-PLAN.md`. Ogni differenza corretta va in `WPF-PARITY-FIXES.md` e committata insieme al fix.
- Docking di riferimento: `TombEditor/MainWindow.xaml` (AvalonDock + `DarkUI.WPF/AvalonDock`), pannelli in `TombEditor/Features/DockableViews`.
- Test su copie dei WAD, mai sugli originali. Lanciare l'app una volta, screenshot, chiudere (l'utente puo' essere attivo).

Regole di lavoro: build Debug x64 dopo ogni blocco, un commit per fix logico, niente Co-Authored-By, file ASCII con CRLF e newline finale, convenzioni di AGENTS.md.

## Fase 0 - Preparazione

- [x] Verificare che il worktree develop esista e compili (build legacy x64 Debug).
- [x] Preparare copie di test in scratchpad (parziale: Andrea1.wad2 TombEngine; altri wad da aggiungere): wad2 TR4 con animazioni/mesh/statics, wad2 vuoto, wad TR3, sprite sequence, file wad2 grande.
- [x] Ricontrollare l'harness `wt.ps1` (guardia foreground, DPI, UIA) dalla memoria "Side by side testing method".
- [x] Creare la matrice di parita' WadTool in `WPF-PARITY-TEST-PLAN.md` (Wave 3, righe WT-1..WT-8, colonne D1-D8).
- [ ] Verificare che la build WPF sia aggiornata prima di ogni test (memoria "stale build").

## Fase 1 - Animation Editor su pannelli dockable

Analisi
- [x] Leggere come Tomb Editor costruisce il docking: `DockingManager`, `LayoutAnchorable`, tema `DarkUI.WPF/Styles/AvalonDock/Theme.xaml`, serializzazione layout e menu Layouts (`LayoutsMenu_SubmenuOpened`).
- [x] Mappare i gruppi di controlli dell'attuale `AnimationEditorWindow.xaml` (638 righe) in pannelli: viewport 3D, lista animazioni, timeline/slider, proprieta' animazione, state changes, anim commands, mesh/bones, blend, ecc.
- [x] Confrontare con il layout legacy (`FormAnimationEditor.cs`, docking DarkUI) per mantenere nomi, posizione iniziale e dimensioni dei pannelli (nomi e disposizione uguali; larghezze colonne allineate a 280/280 come il legacy; altezza timeline 84 contro 38 legacy per i controlli transport, voluta).

Implementazione
- [ ] Creare le view dockable in `WadTool/Features/DockableViews/AnimationEditor/*` (una per pannello, con proprio view model o sotto-VM). Fatto per Animation List, Current Animation, Animation Blending, Skeleton, Transform, Root Motion, Bounding Box (UserControl + PanelStyles.xaml condiviso, test di caricamento XAML con tema DarkUI); Timeline inclusa (TimelinePanel); la viewport resta nel documento centrale. Il view model e' diviso in partial per responsabilita' (Parameters, Playback, Editing, BoundingBox, Tools, Menu, AnimationList); la divisione in VM per pannello resta rinviata (stato condiviso).
- [ ] Spezzare `AnimationEditorWindowViewModel` (partial Menu, AnimationList) in VM per pannello, comunicando via servizi/messenger, senza stato condiviso nascosto.
- [x] Sostituire il layout a griglia con `DockingManager` e tema scuro condiviso.
- [x] Titoli dei pannelli localizzati (titoli inglesi come il resto della finestra, localizzazione globale in Fase 4) (JSON, attenzione ai gotcha delle sezioni) e icone SVG condivise.
- [x] Pannelli chiudibili e riapribili da menu View, con spunta di stato.
- [x] Layout di default identico al legacy (stessa disposizione sinistra/centro/destra); comando "Reset layout".
- [x] Persistenza del layout (salvataggio in configurazione, ripristino, tolleranza a layout corrotto o con pannelli mancanti).
- [x] Float dei pannelli: la viewport HwndHost sta nel documento centrale (CanFloat=False) e Timeline non fluttua, quindi nessun HwndHost viene mai spostato in una finestra flottante; i pannelli laterali sono WPF puri. Il caricamento della finestra reale e' coperto da un test automatico; il drag manuale resta da provare a mano.
- [x] Verificare che la viewport 3D (`PanelRenderingAnimationEditor`) sopravviva a docking, auto-hide, float e minimizzazione (nessuna swap chain persa, nessuna perdita di focus tastiera).
- [x] Shortcut da tastiera funzionanti in ogni pannello e non rubati da textbox.
- [x] Test: layout round trip (4 test automatici DockLayoutHelper: round trip, reset, ri-docking, layout non valido; resize al minimo non provato), pannello nascosto/riaperto, reset, finestra ridimensionata a minimo.
- [x] Valutare se applicare lo stesso modello a Mesh Editor, Skeleton Editor e Static Editor; farlo solo se il legacy ha docking in quelle finestre (verificato: nessun form legacy usa DarkDockPanel, quindi non applicato).

## Fase 2 - Inventario finestre e interazioni (analisi codice legacy)

Per ogni form legacy estrarre da `Form*.cs` e dai controlli usati: handler Mouse*/Key*/DragDrop/ContextMenu, ShortcutKeys, AcceptButton/CancelButton, TabIndex, validazioni, messaggi, stato abilitato/disabilitato, dimensioni minime, persistenza.

- [x] FormMain (finestra principale, browser wad, preview, menu, toolbar, drag and drop file, recent files).
- [x] FormAnimationEditor (timeline, tasti, selezione frame, gizmo, bones, drag in viewport, menu, undo/redo).
- [x] FormMeshEditor.
- [x] FormSkeletonEditor.
- [x] FormStaticEditor.
- [x] FormStateChangesEditor.
- [x] FormAnimCommandsEditor (+ controllo AnimCommandEditor).
- [x] FormReplaceAnimCommands.
- [x] FormAnimationFixer.
- [x] FormBlendCurveEditor (BezierCurveEditor).
- [x] FormSpriteSequenceEditor.
- [x] FormLuaProperties.
- [x] FormNewWad2.
- [x] FormSelectSlot.
- [x] FormOptions.
- [x] Controlli custom: PanelRendering* (Main, Animation, Mesh, Skeleton, Static), PanelTextureMap, tree/list con drag.
- [x] Produrre per ciascuna finestra un elenco "interazione -> codice legacy -> equivalente WPF" in un file `WADTOOL-INTERACTIONS.md` (ASCII).

## Fase 3 - Confronto form per form (ogni riga: legacy vs MVVM)

Per ogni finestra: D1 aspetto, D2 stato iniziale, D3 mouse, D4 tastiera, D5 regole input, D6 flusso dialogo, D7 persistenza, D8 dati salvati identici. Ogni differenza: fix, riga in PARITY-FIXES, commit, rilancio.

- [ ] Main: menu, toolbar, shortcut, browser, preview 3D (orbit/pan/zoom, tasti), apertura/salvataggio/Save As, dirty prompt, recent files, drag and drop.
- [ ] Animation Editor: viewport (mouse e tastiera), timeline (click, drag, frecce, Home/End, play/stop), selezione keyframe, gizmo, bones, copia/incolla, undo/redo, menu completi, filtri e ricerca nella lista, rinomina, drag riordino, importazione/esportazione, opzioni di fix.
- [ ] State Changes Editor: aggiunta/rimozione, dispatch, range frame, validazioni.
- [ ] Anim Commands Editor e Replace Anim Commands.
- [ ] Animation Fixer (scope e opzioni).
- [ ] Blend Curve Editor (punti, maniglie, snap, reset).
- [ ] Mesh Editor: selezione vertici/facce, gizmo, texture map (pan/zoom/selezione), atlas, UV, tool di edit, shortcut.
- [ ] Skeleton Editor: selezione bone, rotazione/spostamento, albero, shortcut.
- [ ] Static Editor: visibilita'/collisione, luci, gizmo, parametri, shortcut.
- [ ] Sprite Sequence Editor: aggiunta/rimozione sprite, ordinamento, preview.
- [ ] Lua Properties.
- [ ] New Wad2 e Select Slot (filtri, doppio click, Enter/Esc).
- [ ] Options.
- [ ] Per tutte: ownership e modalita', Esc/Enter, focus iniziale, tab order, dimensione minima, persistenza posizione, localizzazione, nessun errore di binding nei log.

## Fase 4 - Miglioramento drastico del port MVVM

- [ ] Elencare le differenze trovate e classificarle (bloccante, funzionale, estetica) in `WADTOOL-WPF-GAPS.md`.
- [ ] Rimuovere code-behind di logica nelle finestre: spostare in VM/servizi (service-based, CommunityToolkit).
- [ ] Unificare duplicazioni tra finestre (dialoghi, comandi, validazioni, input numerici) in helper condivisi (TombLib.WPF / DarkUI.WPF).
- [ ] Gestione corretta di undo/redo e dirty state nei VM.
- [ ] Prestazioni: liste grandi virtualizzate, nessun event flood sugli aggiornamenti bulk, Parallel dove sicuro.
- [x] Nullable abilitato e `is null` nei file toccati; rimuovere using inutili (Features e MainWindow: tutti con #nullable enable, zero `== null`, using potati nei partial di MainWindow; rimosso il file vuoto Int16TextConverter.cs).
- [x] Logging NLog di errori e warning da azioni utente (aggiunto nei catch di caricamento animazione, riproduzione suoni, salvataggio texture, suono shatter; restano senza log solo i catch best-effort sulle directory iniziali dei file dialog).
- [ ] Pulizia di handler morti, risorse localizzazione mancanti, binding con errori.

## Fase 5 - Verifica finale

- [x] Build Debug/Release x64 e x86 senza errori (WadTool: Debug x64/x86, Release x64).
- [x] Test WadTool.Tests, TombLib (solo WadTool.Tests 7/7 eseguiti dopo le modifiche; gli altri progetti non toccati), TombEditor, SoundTool tutti verdi; aggiungere test per i fix logici (VM, layout, comandi).
- [x] Audit dipendenze (nessun System.Windows.Forms nel grafo WadTool): nessun using nei sorgenti WadTool; i 5 ProjectReference (DarkUI.WPF, TombLib.WPF, TombLib.Icons, TombLib.Rendering, TombLib) non abilitano UseWindowsForms.
- [ ] Flusso completo su copie: apri wad2, modifica animazione/mesh/static, salva, riapri, confronta con equivalente legacy.
- [x] Controllo log e binding errors a runtime (nessun errore nei log dopo apertura di Animation e Skeleton Editor).
- [x] Controllo ASCII, CRLF, newline finale sui file toccati (tutti conformi; eccezioni preesistenti: BOM in Configuration.cs e PanelRenderingAnimationEditor.cs, Configuration.cs con fine riga LF come upstream).
- [ ] Aggiornare questa checklist e `WPF-PARITY-FIXES.md`; elencare esplicitamente i test non eseguiti (es. DPI 125/150/200 e multi-monitor, che richiedono hardware).

## Ordine consigliato

1. Fase 0 e Fase 2 (analisi, nessun rischio).
2. Fase 1 (cambia la struttura di Animation Editor: meglio farla prima dei confronti su quella finestra).
3. Fase 3 finestra per finestra, partendo da Animation Editor, Main, Mesh/Skeleton/Static, poi i dialoghi piccoli.
4. Fase 4 in parallelo ai fix, Fase 5 alla fine.
