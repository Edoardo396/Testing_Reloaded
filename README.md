# Descrizione
Testing-Reloaded è un programma che aiuta gli insegnanti a consegnare e ritirare compiti svolti su computer.  
L'insegnante inserisce sull'applicazione server il nome del test, il tempo a disposizione e la documentazione che può essere passata agli studenti (potrebbe essere il testo della verifica).  
A questo punto gli studenti si connettono al server (utilizzando l'applicazione client): automaticamente viene scricata la documentazione e creata la cartella del test sul client e parte il tempo.  
Allo scadere del tempo il programma si può comportare in 2 modi, dipendentemente da come viene settato all'inizio: il docente può attivare la consegna automatica oppure scegliere solo di ricevere una notifica.  
Quando lo studente clicca 'Consegna' tutti i file nella cartella del Test (**esclusa la cartella Documentazione**) vengono inviati al server e inseriti nella cartella delle consegne.  

**Attenzione: il firewall può causare malfunzionamenti, disattivarlo**

# Build
La compilazione richiede Visual Studio 2017+, Windows Forms e una connessione a internet per scaricare le dipendenze.
```
$ git clone https://github.com/edofullin/testing-reloaded.git
```
Poi aprire la soluzione visual studio, attendere che nuget installi i pacchetti richiesti e procedere con la compilazione di Server,Client e SharedLibrary

# Documentazione
TODO


