﻿import React, {useState} from 'react';
import {useNavigate} from 'react-router-dom';
import axios from '../../axios';
import './Form.scss';

import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';

const LoginPage = () => {
    const [visibility, setVisibility] = useState<boolean>(false);
    const navigate = useNavigate();
    const query = window.location.search;
    
    const onSubmit = () => {
        axios.get("http://localhost:44405/oauth2/authorize" + query).then((data) => {
            if (data.status === 200) {
                axios.post("http://localhost:44405/oauth2/authorize" + query).then((res) => {
                    if(res.data && res.status === 200) {
                        window.location.assign(res.data);
                    }
                });
            }
        }).catch((err) => {
            console.log(err);
        })
    };

    return (
        <div className="container">
            <div className="content">
                <div className="form">
                    <div className="header">Sign in to your account</div>
                    <form>
                        <div className="input_block">
                            <input type="text" name="useremail" placeholder="Username or email"/>
                        </div>
                        <div className="input_block">
                            <input type={visibility ? "text" : "password"} name="password" placeholder="Password"/>
                            <button type="button" className="visibility" onMouseDown={() => setVisibility(true)}
                            onMouseUp={() => setVisibility(false)}
                            >
                                {visibility ? (
                                    <VisibilityOffIcon/>
                                ) : (
                                    <VisibilityIcon/>
                                )}
                            </button>
                        </div>
                        <div className="button_block">
                            <button type="button" onClick={onSubmit}>Sign in</button>
                        </div>
                        <div className="button_block secondary">
                            <button type="button" onClick={() => navigate("/authorize/register")}>Register account</button>
                        </div>
                        <div className="link_block">
                            <div>Forgot your password?
                            <span onClick={() => navigate("/change-password")}>Change password</span>
                            </div>
                        </div>
                    </form>
                </div>
                <div className="helper"></div>
            </div>
        </div>
    );
};

export default LoginPage;
