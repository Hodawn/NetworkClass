const { json } = require('body-parser');
const { JsonWebTokenError } = require('jsonwebtoken');
const WebSocket=require('ws');

class GameServer{
    constructor(port){

    this.wss=new WebSocket.Server({port});
    this.clients=new Set();
    this.players=new Map();

    console.log(`게임 서버 포트 ${port}에서 시작되었습니다.`);
    }

    setupServerEvent(){
        this.wws.on(`connection`, (socket)=>{
            //1차 기본 접속 처리
            this.clients.add(socket);

            //2차 플레이어ID 생성 및 관리
            const playerId=this.generatePlayerid();

            this.players.set(playerId,
                {
                    socket: socket,
                    position: {x :0, y:0, z:0}
                }
            );
            console.log(`클라이언트 접속 ! ID: ${playerId}, 현재 접속자: ${this.clients.size}`);

            //접속한 플레이어에게 ID 전송 및 메세지
            const welcomeData={
                type: `connection`,
                playerId: playerId,
                message: `서버에 연결되었습니다!`
            };
            socket.send(JSON.stringify(welcomeData));

            socket.on('message', (message)=>{
                try{
                    const data=JSON.parse(message);

                    const messageString=iconv.decode(Buffer.from(data.message), 'euc-kr');
                    console.log(`수신된  메세지: `, messageString);
                    this.broadcast({
                        type: `chat`,
                        message: messageString

                    });

                }catch (error){
                    const messageString=iconv.decode(message, 'euc-kr');
                    console.log(`수진된 메세지: `,messageString);
                    this.broadcast({
                        type:`chat`,
                        message: messageString
                    });
                }

                 
            });
            socket.on(`close`, ()=>{
                this.clients.delete(socket);        //클라이언트 제거

                this.players.delete(playerId);
                this.broadcast({
                    type: 'playerDisconnect',
                    playerId: playerId
                });
                console.log(`클라이언트 퇴장 ID: ${playerId}, 현재 접속자ㅣ ${this.clients.size}`);
            });
        })
    }

    broadcast(data){
        const message=JSON.stringify(data);
        this.clients.forEach(clients=>{
            if(clients.readyState===WebSocket.OPEN)
                {
                this.clients.send(message);
            }
        });
    }
    generatePlayerid(){
        return `player_`+ Math.random().toString(36),substr(2,9);
    }
}

const gameServer = new GameServer(3000);