<center>
# FPGAWingsOfWar
Infomation Processing Project for Imperial EEE/EIE 2023/24

---

**_Sandro Enukidze, Varun Chaganty, Joshua Ewusie-Nelson, Zequan Li, Adam El Jaafari, Deniz Goy_**

---

</center>

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
[![FPGAWingsOfWar Video](https://img.youtube.com/vi/p6NtJfPnt88/0.jpg)](https://youtu.be/p6NtJfPnt88)
