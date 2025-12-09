'use client';

import React, { useEffect, useMemo, useState } from 'react';
import { register, RegisterRequest } from './register';
import './register.css';

const RegisterPage: React.FC = () => {
  const [userName, setUserName] = useState('');
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [gender, setGender] = useState('Male');
  const [dateOfBirth, setDateOfBirth] = useState(''); // yyyy-MM-dd

  const [password, setPassword] = useState('');
  const [repass, setRepass] = useState('');

  const [pwdHint, setPwdHint] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // password strength
  useEffect(() => {
    if (!password) { setPwdHint(''); return; }
    const score =
      (password.length >= 8 ? 1 : 0) +
      (/[A-Z]/.test(password) ? 1 : 0) +
      (/[a-z]/.test(password) ? 1 : 0) +
      (/\d/.test(password) ? 1 : 0) +
      (/[^A-Za-z0-9]/.test(password) ? 1 : 0);
    if (score <= 2) setPwdHint('Mật khẩu yếu');
    else if (score === 3) setPwdHint('Mật khẩu trung bình');
    else setPwdHint('Mật khẩu mạnh');
  }, [password]);

  // animations/eyes
  useEffect(() => {
    const pwdField = document.getElementById('password') as HTMLInputElement | null;
    const repwdField = document.getElementById('repassword') as HTMLInputElement | null;

    const eyes = () => document.getElementsByClassName('eye') as HTMLCollectionOf<HTMLElement>;
    const face = () => document.getElementById('face') as HTMLElement | null;
    const ball = () => document.getElementById('ball') as HTMLElement | null;

    const handleMouseMove = (event: MouseEvent) => {
      if (
        !document.querySelector('#password:is(:focus)') &&
        !document.querySelector('#password:is(:user-invalid)') &&
        !document.querySelector('#repassword:is(:focus)') &&
        !document.querySelector('#repassword:is(:user-invalid)')
      ) {
        const es = eyes();
        for (let eye of es) {
          const x = eye.getBoundingClientRect().left + 10;
          const y = eye.getBoundingClientRect().top + 10;
          const rad = Math.atan2(event.pageX - x, event.pageY - y);
          const rot = (rad * (180 / Math.PI) * -1) + 180;
          eye.style.transform = `rotate(${rot}deg)`;
        }
      }
    };

    const focusHandler = () => {
      const f = face(); if (f) f.style.transform = 'translateX(30px)';
      const es = eyes(); for (let eye of es) eye.style.transform = 'rotate(100deg)';
    };

    const focusOutHandler = (event: FocusEvent) => {
      const f = face(); const b = ball();
      if (f) f.style.transform = 'translateX(0)';
      const target = event.target as HTMLInputElement;
      if (b) b.classList.toggle('sad');
      if (!target.checkValidity()) {
        const es = eyes(); for (let eye of es) eye.style.transform = 'rotate(215deg)';
      }
    };

    const handleSubmitHover = () => {
      const b = ball(); if (b) b.classList.toggle('look_at');
    };

    document.addEventListener('mousemove', handleMouseMove);
    pwdField?.addEventListener('focus', focusHandler);
    pwdField?.addEventListener('focusout', focusOutHandler);
    repwdField?.addEventListener('focus', focusHandler);
    repwdField?.addEventListener('focusout', focusOutHandler);

    const submitBtn = document.getElementById('submit') as HTMLElement | null;
    submitBtn?.addEventListener('mouseover', handleSubmitHover);
    submitBtn?.addEventListener('mouseout', handleSubmitHover);

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      pwdField?.removeEventListener('focus', focusHandler);
      pwdField?.removeEventListener('focusout', focusOutHandler);
      repwdField?.removeEventListener('focus', focusHandler);
      repwdField?.removeEventListener('focusout', focusOutHandler);
      submitBtn?.removeEventListener('mouseover', handleSubmitHover);
      submitBtn?.removeEventListener('mouseout', handleSubmitHover);
    };
  }, []);

  const dobIso = useMemo(() => {
    if (!dateOfBirth) return '';
    const [y, m, d] = dateOfBirth.split('-').map(Number);
    const dt = new Date(y, (m ?? 1) - 1, d ?? 1, 0, 0, 0);
    return dt.toISOString();
  }, [dateOfBirth]);

  const validate = () => {
    if (!userName.trim()) return 'Vui lòng nhập tên đăng nhập';
    if (!fullName.trim()) return 'Vui lòng nhập họ và tên';
    if (!email.trim()) return 'Vui lòng nhập email';
    if (!/^\S+@\S+\.\S+$/.test(email)) return 'Email không hợp lệ';
    if (!phoneNumber.trim()) return 'Vui lòng nhập số điện thoại';
    if (!/^[0-9+\-() ]{8,20}$/.test(phoneNumber)) return 'Số điện thoại không hợp lệ';
    if (!password || password.length < 6) return 'Mật khẩu tối thiểu 6 ký tự';
    if (password !== repass) return 'Mật khẩu nhập lại không khớp';
    if (!dateOfBirth) return 'Vui lòng chọn ngày sinh';
    return '';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    const v = validate();
    if (v) { setError(v); return; }

    setIsLoading(true);

    const payload: RegisterRequest = {
      userName: userName.trim(),
      password,
      email: email.trim(),
      phoneNumber: phoneNumber.trim(),
      fullName: fullName.trim(),
      dateOfBirth: dobIso,
      gender,
    };

    const result = await register(payload);

    if (result.errorCode === 200) {
      window.location.href = '/business/login';
    } else {
      setError(result.errorMessager || 'Đăng ký thất bại');
      setIsLoading(false);
    }
  };

  return (
    <main>
      <section className="form">
        <div className="logo" aria-hidden="true">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round"
              d="M21 7.5l-2.25-1.313M21 7.5v2.25m0-2.25l-2.25 1.313M3 7.5l2.25-1.313M3 7.5l2.25 1.313M3 7.5v2.25m9 3l2.25-1.313M12 12.75l-2.25-1.313M12 12.75V15m0 6.75l2.25-1.313M12 21.75V19.5m0 2.25l-2.25-1.313m0-16.875L12 2.25l2.25 1.313M21 14.25v2.25l-2.25 1.313m-13.5 0L3 16.5v-2.25" />
          </svg>
        </div>
        <h1 className="form__title">Đăng ký tài khoản</h1>
        <p className="form__description">Chào mừng bạn! Vui lòng nhập đầy đủ thông tin để tạo tài khoản</p>

        <form onSubmit={handleSubmit}>
          <label className="form-control__label">Tên đăng nhập</label>
          <input type="text" className="form-control" value={userName} onChange={(e) => setUserName(e.target.value)} required />

          <label className="form-control__label">Họ và tên</label>
          <input type="text" className="form-control" value={fullName} onChange={(e) => setFullName(e.target.value)} required />

          <label className="form-control__label">Email</label>
          <input type="email" className="form-control" value={email} onChange={(e) => setEmail(e.target.value)} required placeholder="you@example.com" />

          <label className="form-control__label">Số điện thoại</label>
          <input type="tel" className="form-control" value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} required placeholder="0901234567" />

          <label className="form-control__label">Giới tính</label>
          <select className="form-control" value={gender} onChange={(e) => setGender(e.target.value)} required>
            <option value="Male">Nam</option>
            <option value="Female">Nữ</option>
            <option value="Other">Khác</option>
          </select>

          <label className="form-control__label">Ngày sinh</label>
          <input type="date" className="form-control" value={dateOfBirth} onChange={(e) => setDateOfBirth(e.target.value)} required />

          <label className="form-control__label">Mật khẩu</label>
          <div className="password-field">
            <input type="password" className="form-control" minLength={6} id="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round"
                d="M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88" />
            </svg>
          </div>
          {pwdHint && <p className="form__description" style={{ marginTop: -20 }}>{pwdHint}</p>}

          <label className="form-control__label">Nhập lại mật khẩu</label>
          <div className="password-field">
            <input type="password" className="form-control" minLength={6} id="repassword" value={repass} onChange={(e) => setRepass(e.target.value)} required />
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round"
                d="M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88" />
            </svg>
          </div>

          {error && <p className="form__error" style={{ color: 'red' }}>{error}</p>}

          <button type="submit" className="form__submit" id="submit" disabled={isLoading}>
            {isLoading ? 'Đang tạo tài khoản...' : 'Đăng ký'}
          </button>
        </form>

        <p className="form__footer">
          Đã có tài khoản? <a href="/business/login">Đăng nhập</a>
        </p>
      </section>

      <section className="form__animation" aria-hidden="true">
        <div id="ball">
          <div className="ball">
            <div id="face">
              <div className="ball__eyes">
                <div className="eye_wrap"><span className="eye"></span></div>
                <div className="eye_wrap"><span className="eye"></span></div>
              </div>
              <div className="ball__mouth"></div>
            </div>
          </div>
        </div>
        <div className="ball__shadow"></div>
      </section>
    </main>
  );
};

export default RegisterPage;
