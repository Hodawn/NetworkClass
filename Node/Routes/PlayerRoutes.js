const express = require('express');
const fs=require('fs');     //fs 모듈 추가
const router = express.Router();

const initalResources = {
    metal : 500,
    crystal : 300,
    deuterirum : 100,
}
router.post('/login', (req, res)=>{
    const {name, password}= req.body;

    if(!global.players[name]){
        return res.status(404).send({message: '플레이어를 찾을 수 없습니다.'});
    }
    if(password !== global.players[name].password){
        return res.status(401).send({message:'비밀번호가 틀렸습니다'});
    }
    const responsePayload = {
        playerName: player.playerName,
        metal: player.resources.metal,
        crystal: player.resources.crystal,
        deuterirum: player.resources.deuterirum
    }
    console.log("Login response payload:", responsePayload);
    res.send(responsePayload);
})

//플레이어 등록 (https://localhost:4000/api/register)
router.post('register', (req, res)=>{
    const {name, password}=req.body;

    if(global.players[name]){
        return res.status(400).send({message : '이미 등록된 사용자입니다.'});
    }

    global.players[name]={
        playerName: name,
        password: password,
        resources:{
            metal:500,
            crystal:300,
            deuterirum:100
        },
        planets:[]
    };

    saveResouces();
    res.send({message: '등록 완료', player:name});
})
//router.post()

//글로벌 플레이어 객체 초기화
global.players={};          //글로벌 객체 초기화

module.exports = router;        //라우터 내보내기