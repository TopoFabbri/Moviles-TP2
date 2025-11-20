# Moviles-TP2

## Game Design Document: DroneRacer ##
**Project Name:** DroneRacer

**Play Store:** [DroneRacer on Playstore](https://play.google.com/store/apps/details?id=com.Fabbri2025.DroneRacer)

**Platform:** Android (Unity)

**Genre:** Minimalist Drone Simulation / Racing
**Developer:** Mateo Fabbri

##### 1. Executive Summary
   **DroneRacer** is a mobile drone simulation game that bridges the gap between arcade racing and realistic physics. The game challenges players to pilot a drone through abstract, floating obstacle courses against the clock. It features a minimalist "Low Poly" aesthetic and leverages mobile gyroscope technology to provide an immersive flight experience.

##### 2. Core Gameplay

   The core loop revolves to a "Race, Earn, Upgrade" cycle. The player must navigate through a series of 3D rings (checkpoints) to complete the track before time runs out.

######    Controls & Mechanics

   -	Gyroscope Steering: The game utilizes the phone's accelerometer/gyroscope for pitch and roll, simulating the feeling of balancing a real drone.
   -	Calibration: A dedicated "Calibrate" feature allows players to set their comfortable "zero" position, ensuring accessibility regardless of how the player holds the device.
   -	On-Screen UI: Virtual joysticks and buttons provide control over throttle (altitude) and yaw (rotation), complementing the motion controls.

######    Progression & Economy

   -	**Time Tiers:** Upon finishing a race, the player’s performance is graded against three tiers:
         - Normal Time
         - Nice Time
         - Ace Time (Highest reward)
   -	**Upgrade System:** Completing races awards **Credits**. Players spend Credits in the "Store" to upgrade the drone's **Maximum Speed.** (Future updates will include Acceleration upgrades).

##### 3. Key Features

   -	**Google Play Services Integration:** Fully integrated ecosystem including:
        - **Leaderboards:** Competitive tracking of best times.
        - **Achievements:** Rewards for milestones (e.g., First Win, Ace Time).
   -	**Visual Style:** A clean, minimalist low-poly art style using high-contrast neon colors for rings against a distinct horizon, ensuring high visibility on small screens.
