import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import axios from "../../axios";

import FormLayout from "../../layout/FormLayout/FormLayout";
import "./Recover.scss";
import VisibilityOffIcon from "@mui/icons-material/VisibilityOff";
import VisibilityIcon from "@mui/icons-material/Visibility";

export const Recover = () => {
	const [visibility, setVisibility] = useState<boolean>(false);
	const navigate = useNavigate();
	const query = window.location.search;
	const isRecover: boolean = query.includes("?token=");
	const recoverToken = query.split("?token=")[1];

	const {
		register,
		handleSubmit,
		setError,
		formState: { errors, isValid },
	} = useForm({
		defaultValues: {
			login: "",
			password: "",
			cpassword: "",
		},
		mode: "onChange",
	});

	const onSubmit = async (values: any) => {
		if (isRecover) {
			if (values.password.toString() !== values.cpassword.toString()) {
				setError("password", {
					type: "validate",
					message: "Password's do not match",
				});
				setError("cpassword", {
					type: "validate",
					message: "Password's do not match",
				});
				return;
			}
		}

		let data = {};
		if (values.login && !isRecover) {
			values.login.includes("@")
				? (data = {
						email: values.login,
				  })
				: (data = {
						username: values.login,
				  });
		} else if (
			isRecover &&
			values.password &&
			values.password === values.cpassword
		) {
			data = {
				password: values.password,
				token: recoverToken,
			};
		}

		!isRecover
			? axios
					.post("/api/v2/recover/token", data)
					.then(res => {
						console.log(res);
						// window.location.assign(window.location.href + "?token=" + res.data);
					})
					.catch(err => {
						setError("login", {
							type: "error",
							message: "User not found. Please try again",
						});
					})
			: axios.post("/api/v2/recover/password", data).then(resData => {
					console.log(resData);
			  });
	};

	return (
		<div className="container">
			<FormLayout>
				{isRecover ? (
					<div className="form">
						<div className="header">Confirm password changing</div>
						<form onSubmit={handleSubmit(onSubmit)}>
							<div
								className={`input_block ${
									errors.password?.message ? "errored" : null
								}`}
							>
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
								{errors.password?.message ? (
									<span className="error_msg">{errors.password?.message}</span>
								) : null}
								<button
									type="button"
									className="visibility"
									onMouseDown={() => setVisibility(true)}
									onMouseUp={() => setVisibility(false)}
								>
									{visibility ? <VisibilityOffIcon /> : <VisibilityIcon />}
								</button>
							</div>
							<div
								className={`input_block ${
									errors.cpassword?.message ? "errored" : null
								}`}
							>
								<input
									type={visibility ? "text" : "password"}
									placeholder="Confirm password"
									{...register("cpassword", {
										required: "Confirm password required",
										minLength: {
											value: 5,
											message: "Password must be at least 5 characters",
										},
									})}
								/>
								{errors.cpassword?.message ? (
									<span className="error_msg">{errors.cpassword?.message}</span>
								) : null}
							</div>
							<div className="button_block secondary">
								<button type="submit">Confirm and change</button>
							</div>
						</form>
					</div>
				) : (
					<div className="form">
						<div className="header">Change password</div>
						<form onSubmit={handleSubmit(onSubmit)}>
							<div
								className={`input_block ${
									errors.login?.message ? "errored" : null
								}`}
							>
								<input
									type="text"
									autoComplete="email"
									placeholder="Username or email"
									{...register("login", {
										required: "Username or email required",
									})}
								/>
								{errors.login?.message ? (
									<span className="error_msg">{errors.login?.message}</span>
								) : null}
							</div>
							<div className="button_block secondary">
								<button type="submit">Request recovery</button>
							</div>
							<div className="link_block">
								<div
									className="back-btn"
									onClick={() => navigate("/authorize" + query)}
								>
									Back to login
								</div>
							</div>
						</form>
					</div>
				)}
				;
			</FormLayout>
		</div>
	);
};
