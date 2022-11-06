import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import axios from "../../axios";
import "./Form.scss";

import VisibilityIcon from "@mui/icons-material/Visibility";
import VisibilityOffIcon from "@mui/icons-material/VisibilityOff";
import FormLayout from "../../layout/FormLayout/FormLayout";

const LoginPage = () => {
	const [visibility, setVisibility] = useState<boolean>(false);
	const navigate = useNavigate();
	const query = window.location.search;

	const {
		register,
		handleSubmit,
		formState: { errors: isValid },
	} = useForm({
		defaultValues: {
			login: "",
			password: "",
		},
		mode: "onChange",
	});

	const onSubmit = async (values: any) => {
		let data = {};
		if (values.login && values.password) {
			values.login.includes("@")
				? (data = {
						email: values.login,
						password: values.password,
				  })
				: (data = {
						username: values.login,
						password: values.password,
				  });

			axios.post("/api/v2/credentials", data).then(codeRes => {
				axios
					.post(`/api/v2/authorize${query}&code=${codeRes.data.code}`)
					.then(res => {
						window.location.assign(res.data);
					});
			});
		}
	};

	return (
		<div className="container">
			<FormLayout>
				<div className="form">
					<div className="header">Sign in to your account</div>
					<form onSubmit={handleSubmit(onSubmit)}>
						<div className="input_block">
							<input
								type="text"
								autoComplete="email"
								placeholder="Username or email"
								{...register("login", {
									required: "Username or email required",
								})}
							/>
						</div>
						<div className="input_block">
							<input
								type={visibility ? "text" : "password"}
								placeholder="Password"
								{...register("password", {
									required: "Password required",
									minLength: {
										value: 5,
										message: "Password must be at least 5 characters",
									},
								})}
							/>
							<button
								type="button"
								className="visibility"
								onMouseDown={() => setVisibility(true)}
								onMouseUp={() => setVisibility(false)}
							>
								{visibility ? <VisibilityOffIcon /> : <VisibilityIcon />}
							</button>
						</div>
						<div className="button_block">
							<button type="submit" disabled={!isValid}>
								Sign in
							</button>
						</div>
						<div className="button_block secondary">
							<button
								type="button"
								onClick={() => navigate("/authorize/register" + query)}
							>
								Register account
							</button>
						</div>
						<div className="link_block">
							<div>
								Forgot your password?
								<span onClick={() => navigate("/recover")}>
									Change password
								</span>
							</div>
						</div>
					</form>
				</div>
			</FormLayout>
		</div>
	);
};

export default LoginPage;
