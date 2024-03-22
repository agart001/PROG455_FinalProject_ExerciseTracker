# Workout Progress Tracker

## Final Project Proposal by Alex Gartner

---

### App Overview

- The app is a simple web-based service designed for tracking workout progress.
- Users create accounts to track their progress on different exercises.
- Users can input exercises, define forms and descriptions, and log progress by entering reps and sets per session.
- Visual representations of progress, such as graphs or tables, are available based on user preference.

---

### Client Overview

- Utilize C# .NET MVC as the foundational framework for the client.
- Receive user inputs from the web interface to modify, display, and save data.
- Handle communication with the database storing user data.
- Provide graph/table data to the web interface.

---

### Web Overview

- The web app serves as the interface for user interaction with the client.
- Features a clean and intuitive UI, with CSS targeting mobile devices as the primary user interface.
- Capable of receiving and displaying graph/table data from the client.
- Users can add/remove/edit various elements of their data, including exercises, set/rep data, and user information.

---

### Database Overview

- Responsible for storing user and exercise data.
- Comprised of three tables:
  - User table: Stores user information (name/email/id).
  - Exercise table: Stores exercise information (name/desc/id), linked to users via foreign key.
  - Rep/Set table: Stores exercise data, linked to users and exercises via foreign keys.

---

### Examples

---

### Fun Optional Ideas

- **Gamification:**
  - Add points and rewards for progress.
  - Track levels/xp.
- **Theme:**
  - Fantasy Warrior.
  - Space Ranger.
- **Animations:**
  - Enhance UI feel with polished animations.