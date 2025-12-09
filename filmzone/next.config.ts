/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    // dùng một trong hai, remotePatterns linh hoạt hơn
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'res.cloudinary.com',
      },
    ],
    // domains: ['res.cloudinary.com'],
  },
};

module.exports = nextConfig;
