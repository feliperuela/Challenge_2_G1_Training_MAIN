# Technical Challenge: Unitree G1 Humanoid - Static Balance Policy

*Candidate*: Felipe Ruela

This repository contains my solution for **Challenge 2: Reinforcement Learning with Game Engines**, as part of the evaluation process for the Humanoid Robotics Engineer position at CloudWalk.

This project documents the development of a policy for a Unitree G1 humanoid to achieve and maintain stable static balance in a simulated Unity environment using Reinforcement Learning.

> **Note:** A full project `.zip` file, including the Unity `Library` folder for faster import, is available in the [Releases section](https://github.com/feliperuela/Challenge_2_G1_Training_MAIN/releases).

---

## 1. Core Technical Strategy & Design Choices

To solve the complex task of humanoid balancing, I implemented a strategy focused on ensuring a stable and efficient learning process.

* **Automated Curriculum Learning:** I engineered a custom `GravityController.cs` script that automatically adjusts the simulation's gravity based on the agent's training progress. The agent begins its training in a low-gravity "easy mode" to learn the fundamentals of balance, and the difficulty is progressively increased as it becomes more proficient. This automated curriculum was critical for overcoming the initial high difficulty of the task and achieving successful convergence.

* **Advanced Reward Shaping:** The agent's reward function in `G1_Agent.cs` incorporates a dynamic **instability penalty**. The agent receives negative rewards only if its balance *worsens* from one step to the next, but only when it is already in a somewhat unstable state. This teaches the agent to actively avoid movements that lead to falling, resulting in a more stable and robust final policy.

* **Reproducible Pipeline:** The entire environment setup, from Python dependencies to training configuration, is codified. A PowerShell script is provided to create the Python virtual environment, and the `mlagents-learn` command uses a version-controlled `.yaml` file, ensuring that the training process is fully reproducible.

---

## 2. Live Demonstration & Results

The agent was successfully trained, mastering the static balance task. The final policy allows the robot to autonomously and consistently counteract physics-based instabilities.

### Trained Agent in Action

[![G1 Humanoid Static Balance Demonstration](1_Movie/demonstration.jpg)](https://youtu.be/ccoovZJOb4Y)

*(Click the image above to watch the live demonstration of the trained agent)*

### Performance Metrics (Learning Curve)

The training progress was logged using TensorBoard. The cumulative reward curve demonstrates a successful learning progression, moving from initial failure to a stable, high-performance policy.

---

## 3. Step-by-Step Reproduction Guide

This guide provides all necessary steps to set up the environment from scratch and reproduce the results.

### A. Prerequisites

* **Unity Hub**
* **Unity Editor:** Version **[2021.3.45f1 (LTS)](https://unity.com/pt/releases/editor/whats-new/2021.3.45f1#installs)** is required. Using this specific version was crucial for physics stability with the URDF importer.
* **Python:** Version **3.10.11** was used.
* **ML-Agents:** This project uses `com.unity.ml-agents` (Version 2.0.1).

### B. Required Dependencies

Before setting up the Unity project, please obtain the following external packages:

1.  **URDF-Importer for Unity:**
    * Download the official release from the Unity Technologies GitHub repository or release file.
    * **Link:** `https://github.com/Unity-Technologies/URDF-Importer/releases`

2.  **Unitree ROS Description Files:**
    * The robot's URDF description files are required. You can clone or download them from the official repository.
    * **Link:** `https://github.com/unitreerobotics/unitree_ros`

### C. Unity Project Setup

1.  Clone this repository to your local machine.
2.  Install Unity Hub and the specified Unity Editor version (2021.3.45f1).
3.  Add the cloned project folder (`G1_Agent_Training_V6`) to your Unity Hub projects list and open it. Click "Continue" if any package error messages appear.
4.  Once the project is open, navigate to **Window > Package Manager**.
5.  Install the required packages:
    * **ML-Agents:** Click the **'+'** icon, select **"Add package by name..."**, and enter `com.unity.ml-agents`.
    * **URDF Importer:** Click the **'+'** icon, select **"Add package from disk..."**, and navigate to the `package.json` file inside the `URDF-Importer` folder you downloaded in the previous step.

### D. Python Environment and Training

1.  Open a **PowerShell** terminal and run the provided script to set up the Python virtual environment and start the training process:
    * `.\Run-Train-With-Activation.cmd`
2.  Wait for the message **`Start training by pressing the Play button in the Unity Editor.`** to appear in the terminal.
3.  Press the **Play** button in the Unity Editor to begin training.
4.  To monitor the learning curve, open a separate terminal and run:
    * `.\Run-TensorBoard.cmd`
5.  To stop the training, press the **Play** button again in the Unity Editor and then press **CTRL+C** in the training terminal.

---

## 4. Insights & Limitations

* **Insight:** The automated curriculum was the single most critical factor for success. Initial attempts to train the agent in a full-gravity environment (`-9.81 m/s²`) consistently failed, as the agent could not survive long enough to acquire a useful policy. Simplifying the initial task was essential for bootstrapping the learning process.
* **Limitation:** The current policy is specialized for static balance on a flat, stationary surface. It has not been trained for locomotion or to handle external perturbations.
* **Next Steps:** The logical next phase is to build upon this stable balancing policy to teach locomotion. This will involve evolving the reward function to incentivize forward velocity and expanding the curriculum to include more challenging scenarios with full gravity.

---

## 5. Future Work & Advanced Concepts

Beyond the current implementation, I have conceptualized an advanced hardware-inspired solution to dramatically improve the robot's stability: a [**Gyroscopic Stabilizer Backpack (Control Moment Gyroscope - CMG)**](https://www.youtube.com/watch?v=cquvA_IpEsA).

* **Concept:** A fast-spinning flywheel mounted on a gimbaled frame inside a backpack module. By precisely tilting the flywheel's spin axis, we can induce gyroscopic precession, generating powerful and near-instantaneous corrective torques on the robot's torso.
* **Advantages:** This approach, common in aerospace applications like, could offer faster reaction times and higher energy efficiency for maintaining balance compared to relying solely on leg motor actuators.
* **Next Steps:** A future research direction would be to model this system in simulation, design the necessary control algorithms to avoid singularities, and integrate it into the agent's observation and action space. This represents a promising path towards achieving human-level agility and disturbance rejection.
