<center>

# FPGAWingsOfWar
Infomation Processing Project for Imperial EEE/EIE 2023/24

---

**_Sandro Enukidze, Varun Chaganty, Joshua Ewusie-Nelson, Zequan Li, Adam El Jaafari, Deniz Goy_**

---

</center>

Please see our project write up for details behind the engineering behind the **FPGAWingsOfWar** [`Information Processing Group 11 Report.pdf`](./Information%20Processing%20Group%2011%20Report.pdf)
## Contribution Table

**Key:** o = Main Contributor; v = Co-Author


| Task                | Files                                                                                                                                     | Sandro | Varun | Joshua | Zequan | Adam | Deniz |
|:--------------------|:------------------------------------------------------------------------------------------------------------------------------------------|:--------:|:-----:|:------:|:-----:|:----:|:----------:|
| AWS (Frontend & Backend)             | [`server.py`](AWS/server.py)                                                                                               |          |       |        |   o   |      |            |
| Quartus FPGA design           | [`DE10_LITE_Golden_Top.sof`](Quartus/dogfight/DE10_LITE_Golden_Top.sof), [`NiosClient.py`](Quartus/dogfight/software/dog_sw_test4/NiosClient.py), [`UnityServer.py`](Quartus/dogfight/software/dog_sw_test4/UnityServer.py)   |     |       |    o     |       |       |    o     |
| Unity Game                  | [`FlightMultiplayer-master.sln`](Unity/FlightMultiplayer-master/FlightMultiplayer-master/FlightMultiplayer-master.sln)              |   o   |   o   |        |       |   o   |             |

___
## Directory Structure
This is the directory structure that was used for the project.

Directory    | Use
:-----------:|:------------------------------------------------
[`AWS`](./AWS/)     | Webpage running on a EC2 instance
[`Quartus`](./Quartus/)     | FPGA design, Nios client and Unity Server code
[`Unity`](./Unity/)         | Unity video game

## Results
Below you can see a video of the project. 
___
[![FPGAWingsOfWar Video](https://img.youtube.com/vi/p6NtJfPnt88/0.jpg)](https://youtu.be/p6NtJfPnt88)


___
## Contributors

Name    | Email | Github User
:-----------:|:-----------:|---------------:|
Sandro  | sandro.enukidze22@imperial.ac.uk  | [`Sandro Enukidze`](https://github.com/Sandro-Enukidze-IC)
Varun  | varun.chaganty22@imperial.ac.uk | [`Varun Chaganty`](https://github.com/NightRaven3142)
Joshua  | joshua.ewusie-nelson22@imperial.ac.uk  | [`Joshua Ewusie-Nelson `](https://github.com/E-N-J)
Zequan  | zequan.li22@imperial.ac.uk  | [`Zequan Li`](https://github.com/Cecilialzq)
Adam  | adam.eljaafari22@imperial.ac.uk  | [`Adam El Jaafari`](https://github.com/AEljaa)
Deniz  | deniz.goy22@imperial.ac.uk  | [`Deniz Goy`](https://github.com/DenizzG)

