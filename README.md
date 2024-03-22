---
    title:  >
      PowerPoint Presentation
    date:
    slug: PROG455_FPP
    category:
    author:
---

<a href="PROG455_FPP.pdf">PDF version</a> | <a href="PROG455_FPP.pptx">Powerpoint Version</a>


    

<section typeof='http://purl.org/ontology/bibo/Slide'>
<img src='Slide0.png' alt='Final Project Proposal:Workout Progress Tracker
By Alex Gartner
' title='Slide: 0' border='1'  width='85%%'/>





</section>



<section typeof='http://purl.org/ontology/bibo/Slide'>
<img src='Slide1.png' alt='App Overview
The app is a simple web-based service, where a user creates an account and can track their progress on different exercises.
The user can input different exercises to track, they can define the exercise&#x27;s form and description, then each day/session they can input their progress on the exercise by entering the amount reps and sets they completed.
This exercise progress can be viewed in different visual formats based on preference, i.e., graphs or tables.
' title='Slide: 1' border='1'  width='85%%'/>





</section>



<section typeof='http://purl.org/ontology/bibo/Slide'>
<img src='Slide2.png' alt='Client Overview
I suggest we use C# .NET MVC as the base framework for the client.
The client should be able to receive user inputs from the web interface, then use those inputs to modify, display, and save data.
The client will be responsible for communicating with the database which stores user data.
The client will be responsible for feeding graph/table data to the web interface.
' title='Slide: 2' border='1'  width='85%%'/>





</section>



<section typeof='http://purl.org/ontology/bibo/Slide'>
<img src='Slide3.png' alt='Web Overview
The web app is responsible for providing an interface by which the user interacts with the client.
The web app should have clean and simple UI, the CSS should target phones as the primary user device.
The web app should be able to receive graph/table data from the  client and properly display it.
The user should be able to add/remove/edit different elements of their data. i.e., exercises, set/rep data, user information 
' title='Slide: 3' border='1'  width='85%%'/>





</section>



<section typeof='http://purl.org/ontology/bibo/Slide'>
<img src='Slide4.png' alt='Database Overview
The database is responsible for storing the users and their exercise data.
The database should be comprised of a 3 tables:
 A user table, which stores user information(name/email/id).
 A exercise table which, using a user id as foreign key,  stores the exercise information(name/desc/id). 
A rep/set table which, using user and exercise id as foreign keys, stores the exercise data.

' title='Slide: 4' border='1'  width='85%%'/>





</section>



<section typeof='http://purl.org/ontology/bibo/Slide'>
<img src='Slide5.png' alt='Examples
' title='Slide: 5' border='1'  width='85%%'/>





</section>



<section typeof='http://purl.org/ontology/bibo/Slide'>
<img src='Slide6.png' alt='Fun Optional Ideas
Gamification 
Add points and rewards for progress
Track levels/xp
Theme
Fantasy Warrior 
Space Ranger
Animations 
UI Feel
Polished 
' title='Slide: 6' border='1'  width='85%%'/>





</section>

