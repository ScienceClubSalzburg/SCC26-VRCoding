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

## Snap Turn

Snap Turn wurde geprüft. Im aktuellen XR-Setup ist kein eigener Snap Turn Provider sichtbar.

Die Bewegung erfolgt über Waypoints/Teleportation.

Falls sich beim Testen eine Controller-Drehung zeigt, wird die Turn-Action im Input Action Asset deaktiviert.

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

---

## Finale MVP-Entscheidung: Station "Birthday Party"

### Ziel der Station

Die Spieler:innen entschluesseln den Namen des Geburtstagskindes und geben ihn auf der Geburtstagskarte ein.

Wenn der Name richtig ist, oeffnet sich die Tuer zur naechsten Station.

### Finale Loesung

Der gesuchte Name ist:

**EMILIA**

### Spielprinzip

Eine Person spielt in VR. Eine zweite Person ist ausserhalb von VR und hat eine reale Caesar-/Emoji-Scheibe.

Die VR-Person sieht Symbole im Raum und beschreibt sie. Die reale Person uebersetzt die Symbole mit der Scheibe in Buchstaben. Gemeinsam finden sie den Namen.

### Storylogik

Am Boden liegt eine Einladung mit dem Hinweis:

**"Du bist eingeladen, aber der Name fehlt. Du brauchst Hilfe."**

Auf dem Tisch liegt eine unfertige Geburtstagskarte mit dem Hinweis:

**"Ich bin unfertig. Bring mich zum Drucker."**

Der Drucker erklaert die Aufgabe:

**"Ich brauche den Namen. Der Name ist mit Emojis verschluesselt. Nutzt die Caesar-Scheibe."**

Das Finale findet am Drucker statt.

Wenn der richtige Name eingegeben wurde:

- **"Name erkannt ..."**
- **"Karte wird magisch gedruckt ..."**

Was genau auf den Einladungskarten steht, wird noch bekannt gegeben.

### Code-Logik

Die Symbole auf der Girlande stehen fuer Buchstaben. Die Zahlen ueber den Symbolen geben die Reihenfolge im Namen an.

| Zahl | Buchstabe | Symbol im MVP |
|---:|---|---|
| 1 | E | Kuchen |
| 2 | M | Ballon |
| 3 | I | Stern |
| 4 | L | Geschenk |
| 5 | I | Stern |
| 6 | A | Smile |

Am Eingang liegt ein Teppich-Hinweis:

**A = Smile**

### Eingabe

Am Drucker befindet sich ein Eingabefeld.

Die Spieler:innen geben den Namen ein und druecken einen Button.

MVP-Entscheidung:

**World-Space-Canvas mit TMP_InputField und Submit-Button**

Keine echte VR-Tastatur im MVP.

### Erfolg

Wenn die Eingabe richtig ist:

- Der Drucker erkennt den Namen.
- Die Karte wird magisch gedruckt.
- Konfetti startet.
- Die Tuer oeffnet sich.
- Es erscheint ein Erfolgstext.

Erfolgstext:

**"Richtig! Die Tuer oeffnet sich."**

### Fehler

Wenn die Eingabe falsch ist:

- Die Eingabe wird geloescht.
- Die Tuer bleibt geschlossen.
- Es erscheint ein Hinweistext.

Fehlertext:

**"Falscher Name - probiere es nochmal."**

MVP-Entscheidung:

**Kein kompletter Level-Reset.**

### Raumobjekte im MVP

Diese Objekte muessen vorhanden sein:

- VR-Basisraum mit Waypoint-Area
- Einladung am Boden
- Tisch in der Mitte
- Geburtstagskarte auf dem Tisch
- Drucker
- Eingabefeld am Drucker
- Party-Girlande mit 6 nummerierten Symbolen
- Teppich-Hinweis am Eingang
- Tuer zur naechsten Station
- Konfetti-Effekt

Diese Objekte sind optional:

- Buecherregal
- Kuchen
- Geschenke
- Ballons
- Katze
- Kratzbaum
- Futterschuessel
- Couch

### Nicht Teil des MVP

Nicht Teil der ersten Version:

- perfekte Party-Dekoration
- finale 3D-Assets
- echte VR-Tastatur
- komplette Level-Reset-Logik
- komplexe Animationen
- Sounddesign
- detaillierte Beleuchtung
- Quest-Optimierung im ersten Schritt

### Akzeptanzkriterien

Das MVP gilt als fertig, wenn:

- der Raum in VR begehbar ist
- die Girlande sichtbar ist
- die Zahlen ueber den Symbolen lesbar sind
- die Einladung am Boden sichtbar ist
- die Karte auf dem Tisch sichtbar ist
- der Drucker sichtbar ist
- der Teppich-Hinweis sichtbar ist
- der Name eingegeben werden kann
- falsche Eingabe abgelehnt wird
- richtige Eingabe erkannt wird
- Konfetti startet
- die Tuer sich oeffnet
- keine Compile Errors vorhanden sind
