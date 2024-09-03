from aiohttp import web
import sqlite3
from datetime import datetime

#!/usr/bin/python

class Database:
    # create a database instance
    def __init__(self):
        self.conn = sqlite3.connect('test.db')
        print ("successfully open the database")
        self.cursor = self.conn.cursor()
        self.cursor.execute('''CREATE TABLE IF NOT EXISTS scores
            (id INTEGER PRIMARY KEY AUTOINCREMENT    NOT NULL,
            name          TEXT   NOT NULL,
            score         INT    NOT NULL,
            damage        INT    NOT NULL,
            player_ip     TEXT   NOT NULL,
            time          TEXT   NOT NULL);''')
        
    def insert_score(self, name, score, damage, player_ip, time):
        # now = datetime.now() # current time
        # now_string = now.strftime("%Y/%m/%d, %H:%M:%S")
        sql = "INSERT INTO scores (name, score, damage, player_ip, time) " +\
              "VALUES (?, ?, ?, ?, ?)"
        self.cursor.execute(sql, (name, score, damage, player_ip, time))
        self.conn.commit()

    def get_scores(self, max_count):
        # send the whole database to client
        sql = "SELECT name,score,damage,player_ip,time FROM scores ORDER BY score DESC "
        if max_count > 0:
            sql += " LIMIT %s" % (max_count)
        self.cursor.execute(sql)
        # all the data
        scoreboard = self.cursor.fetchall()
        return scoreboard

    def __del__(self):
        self.conn.commit()
        self.conn.close()


db = Database()

async def get_scores(request):
    # get scores from SQLite database
    return web.Response(text="1,2,3,4,5")


async def update_count(request):
    params = request.rel_url.query
    name = params['name']
    score = params['score']
    damage = params['damage']
    player_ip = params['player_ip']
    time = params['time']

    scores[name] = score
    print(name, score, damage, player_ip, time)
    # update database
    db.insert_score(name, score, damage, player_ip, time)
    return web.Response(text='\n'.join([
        'your name is = ' + name,
        'your score = ' + score,
    ]))


async def get_count(request):
    # at most return max_count
    params = request.rel_url.query
    max_count = -1 # -1 retuen all data
    if 'max_result_count' in params:
        try:
            max_count = int(params['max_result_count'])
        except:
            # max_result_count=3.5
            # if not follow the format return defult value -1
            pass
    #print(scores)
    # [
    #     {score，damage},
    #     {score，damage},
    #     {score，damage},

    # ]
    # get one line of data
    scores = db.get_scores(max_count)
    print(scores)

    res = []
    # transfer all data into JSON format
    for score in scores:
        js = {
              "name": score[0],
              "score": score[1],
              "damage": score[2],
              "player_ip": score[3],
              "time": score[4]
             }
        res.append(js)
    return web.json_response(res)

app = web.Application()
app.add_routes([web.get('/get_scores', get_scores)])
app.add_routes([web.get('/update_count', update_count)])
app.add_routes([web.get('/get_count', get_count)])
app.add_routes([web.static('/', "./webpage", show_index=True)])

web.run_app(app)

# return leaderboard
