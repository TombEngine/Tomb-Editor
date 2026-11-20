# WadTool WPF: differenze note e lavoro residuo

Classificazione: B = bloccante, F = funzionale, E = estetica, T = tecnico/architettura. Stato al 2026-11-20.

## Differenze note rispetto al legacy

| ID | Classe | Descrizione | Stato |
| --- | --- | --- | --- |
| G-1 | E | Altezza della timeline dell'Animation Editor 84 DIP contro 38 del legacy (ospita i pulsanti transport a 24 px). | Voluta, da confermare a vista |
| G-2 | E | Titoli dei pannelli dock in inglese (LayoutAnchorable non e' un DependencyObject, Tomb Editor ha lo stesso limite). | Aperta |
| G-3 | F | Messaggi, popup e testi creati da codice (VM) non sempre localizzati; solo il XAML e' coperto da chiavi. | Aperta |
| G-4 | E | Mesh Editor: dall'anteprima offscreen le intestazioni "Texturing" e "Editing tools" della colonna destra sembrano sovrapposte; non verificato a vista con dati reali. | Da verificare |

## Risolte in questo ciclo

- Pennello Brush_BackgroundSlightlyDark inesistente (barra strumenti bianca, intestazioni senza sfondo): ora Brush_Background_Alternative.
- Riferimenti a Brush_Text_Low inesistente rimossi.
- Larghezze delle colonne dock a 280 come il legacy.
- Lista animazioni: un solo evento Reset a ogni ricostruzione.

## Non verificato (richiede test a vista o hardware)

- Confronto affiancato D1-D8 per Main, Animation Editor (parziale), Mesh, Static, Skeleton, Sprite Sequence: vedi `WPF-PARITY-TEST-PLAN.md` Wave 3.
- Drag manuale di pannelli flottanti dell'Animation Editor.
- DPI 125/150/200 e configurazioni multi-monitor.
- Flusso completo apri wad2, modifica, salva, riapri, confronta con il legacy.

## Architettura residua

- T-1: AnimationEditorWindowViewModel e' diviso in partial per responsabilita' ma non in view model per pannello (stato condiviso).
- T-2: MainWindow resta un code-behind con handler (diviso in partial Menu e Tree); nessun view model completo.
