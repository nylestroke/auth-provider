import React, {useState} from 'react';
import {useNavigate} from 'react-router-dom';
import './../Login/Form.scss';
import VisibilityOffIcon from "@mui/icons-material/VisibilityOff";
import VisibilityIcon from "@mui/icons-material/Visibility";
import {useForm} from "react-hook-form";
import axios from "../../axios";

const RegisterPage = () => {
    const [visibility, setVisibility] = useState<boolean>(false);
    const navigate = useNavigate();
    const query = window.location.search;

    const {register, handleSubmit, setError, formState: {errors: isValid},} = useForm({
        defaultValues: {
            username: "",
            email: "",
            password: "",
            cpassword: "",
        },
        mode: "onChange",
    });

    const onSubmit = async (values: any) => {

        if (values.password.toString() !== values.cpassword.toString()) {
            setError("password", {
                type: "validate",
                message: "Password's do not match"
            });
            setError("cpassword", {
                type: "validate",
                message: "Password's do not match"
            });
            return;
        }

        const data = {
            username: values.username,
            email: values.email,
            password: values.password
        };
        
        axios.post("/api/v2/credentials", data).then(codeRes => {
            axios.post(`/api/v2/authorize${query}&code=${codeRes.data.code}`).then(res => {
                window.location.assign(res.data);
            });
        })
    };

    return (
        <div className="container">
            <div className="content">
                <div className="form">
                    <div className="header">Sign up new account</div>
                    <form onSubmit={handleSubmit(onSubmit)}>
                        <div className="input_block">
                            <input type="text"
                                   placeholder="Username"
                                   {...register('username', {
                                       required: "Username required"
                                   })}
                            />
                        </div>
                        <div className="input_block">
                            <input type="email"
                                   placeholder="Your email"
                                   {...register('email', {
                                       required: "Email required"
                                   })}
                            />
                        </div>
                        <div className="input_block">
                            <input type={visibility ? "text" : "password"}
                                   placeholder="Password"
                                   {...register('password', {
                                       required: "Password required",
                                       minLength: {
                                           value: 5,
                                           message: "Password must be at least 5 characters",
                                       },
                                   })}
                            />
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
                        <div className="input_block">
                            <input type={visibility ? "text" : "password"}
                                   placeholder="Confirm password"
                                   {...register('cpassword', {
                                       required: "Confirm password required",
                                       minLength: {
                                           value: 5,
                                           message: "Password must be at least 5 characters",
                                       },
                                   })}
                            />
                        </div>
                        <div className="button_block">
                            <button type="submit">Sign Up</button>
                        </div>
                        <div className="button_block secondary">
                            <button type="button" onClick={() => navigate("/authorize" + query)}>Back to login</button>
                        </div>
                        <div className="link_block">
                            <div>Do you already have an account?
                                <span onClick={() => navigate("/authorize") + query}>Sign In</span>
                            </div>
                        </div>
                    </form>
                </div>
                <div className="helper"></div>
            </div>
        </div>
    );
};

export default RegisterPage;
