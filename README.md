# SCC26-VRCoding / VR-Labyrinth

## Projektübersicht

Dieses Unity-Projekt ist die technische Grundlage für ein modulares VR-Labyrinth.  
Die einzelnen Räume sollen später als kurze VR-Minispiele bzw. Rätselräume umgesetzt werden.

Alle Räume basieren auf einem gemeinsamen VR-Basisraum. Dadurch sollen Aufbau, Bewegung und Interaktion in allen Räumen einheitlich funktionieren.

---

## Unity-Version

Dieses Projekt wurde erstellt mit:

**Unity 6.0.57f1**

Bitte das Projekt mit genau dieser Unity-Version öffnen, damit es keine Versionskonflikte gibt.

---

## Verwendetes Template

Das Projekt basiert auf dem:

**Unity VR Template**

Das Template stellt bereits eine grundlegende VR-Struktur zur Verfügung, zum Beispiel:

- XR Origin / VR Player
- Controller-Unterstützung
- grundlegende XR Interaction Toolkit Einstellungen
- Teleportation als Bewegungsform

---

## Zielplattform

Die Zielplattform ist:

**Meta Quest / Android**

Das Projekt soll auf einer Meta Quest S3 lauffähig sein.  
Der Android-Build muss getestet werden, bevor die einzelnen Rätselräume weiter ausgebaut werden.

---

## StartszeneTemplate

Die StartszeneTemplate dient als einfacher VR-Basisraum.

In dieser Szene sollen zunächst nur die wichtigsten Grundelemente vorhanden sein:

- Boden
- Wände
- Licht
- XR Origin / VR Player
- Teleportation
- einfacher Ausgang oder Türplatzhalter
- Platz für spätere Rätselobjekte

Die Szene ist noch kein fertiges Spiel, sondern eine technische Grundlage für alle weiteren Räume.

---

## Bewegung in VR

Die Bewegung erfolgt ausschließlich über:

**Teleportation**

Spieler:innen können sich über definierte Teleport-Flächen oder Teleport-Punkte im Raum bewegen.

---

## Kein Continuous Movement

**Continuous Movement wird in diesem Projekt nicht verwendet.**

Das bedeutet:

- kein freies Laufen mit dem Joystick
- keine dauerhafte Vorwärtsbewegung
- weniger Risiko für Motion Sickness
- einfacheres Testen
- einheitliche Bewegung in allen Räumen

---

## Ziel des Basisraums

Der Basisraum soll später als Vorlage für mehrere Rätselräume verwendet werden.

Jeder Raum soll grundsätzlich enthalten:

- einen Eingang
- einen Ausgang
- eine einfache Aufgabe oder ein Rätsel
- Feedback bei richtig/falsch
- eine Tür oder einen Übergang, der nach erfolgreicher Lösung freigeschaltet wird

---

## Arbeitsregel

Zuerst muss der einfache VR-Basisraum auf der Meta Quest funktionieren.

Erst danach werden:

- Rätsel
- Assets
- Sounds
- Animationen
- zusätzliche Effekte

eingebaut.